/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.engine;
import retro.core.di;
import retro.core.functional.interop_function;
import retro.core.async.task;
import retro.core.strings.encoding;
import retro.platform.backend;
import retro.platform.window;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.textures.texture_decoder;
import retro.interop.interop_error;

namespace
{
    using ConfigCallback = void (*)(retro::EngineConfigContext *, void *);

    using DeleteCallback = void (*)(void *);
    using EqualsCallback = bool (*)(void *, void *);
    using WindowRemovedCallback = void (*)(void *, std::uint64_t);

    using WindowCreatedCallback = void (*)(void *, std::uint64_t);
    using ShutdownRequestedCallback = void (*)(void *);
    using OnErrorCallback = void (*)(void *, const char *);

    struct WindowRemovedBinding
    {
        retro::InteropFunction<void(std::uint64_t)> function;

        void operator()(const retro::Window &window) const
        {
            function(window.id());
        }

        bool operator==(const WindowRemovedBinding &other) const noexcept
        {
            return function == other.function;
        }
    };
} // namespace

extern "C"
{
    RETRO_API retro::Engine *retro_create_engine(const retro::PlatformBackendInfo platform_config,
                                                 const ConfigCallback config_callback,
                                                 void *user_data,
                                                 retro::InteropError *error_message)
    {
        return retro::try_execute(
            [&]
            {
                retro::EngineConfigContext context;
                context.services.add(retro::PlatformBackend::create(platform_config));
                context.services.add_singleton<retro::PipelineManager>()
                    .add_singleton<retro::RenderPipeline, retro::GeometryRenderPipeline>()
                    .add_singleton<retro::RenderPipeline, retro::SpriteRenderPipeline>()
                    .add_singleton<retro::AssetManager>()
                    .add_singleton<retro::AssetDecoder, retro::TextureDecoder>();

                if (config_callback != nullptr)
                {
                    config_callback(std::addressof(context), user_data);
                }

                auto engine = std::make_unique<retro::Engine>(context.services.create_service_provider());
                retro::Engine::initialize(*engine);
                return engine.release();
            },
            *error_message);
    }

    RETRO_API void retro_engine_create_main_window(retro::Engine *engine,
                                                   const char16_t *window_title,
                                                   const std::int32_t window_tile_length,
                                                   const std::int32_t width,
                                                   const std::int32_t height,
                                                   const retro::WindowFlags flags,
                                                   void *user_data,
                                                   const WindowCreatedCallback created_callback,
                                                   const OnErrorCallback error_callback)
    {
        std::ignore = [](retro::WindowDesc &&desc,
                         retro::Engine *local_engine,
                         void *local_user_data,
                         const WindowCreatedCallback on_created,
                         const OnErrorCallback on_error) -> retro::Task<>
        {
            try
            {
                const auto window = co_await local_engine->create_new_window(std::move(desc));
                if (!window.has_value())
                {
                    on_error(local_user_data, window.error().message.data());
                }

                on_created(local_user_data, window.value()->id());
            }
            catch (const std::exception &e)
            {
                on_error(local_user_data, e.what());
            }
            catch (...)
            {
                on_error(local_user_data, "Unknown error");
            }
        }(
            retro::WindowDesc{
                .width = width,
                .height = height,
                .title = retro::convert_string<char>(
                    std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                .flags = flags,
            },
            engine,
            user_data,
            created_callback,
            error_callback);
    }

    RETRO_API void retro_destroy_engine(retro::Engine *engine)
    {
        // Put the engine back into a unique_ptr so no matter what happens here it always get destroyed
        std::unique_ptr<retro::Engine> ptr(engine);
        retro::Engine::shutdown();
    }

    RETRO_API bool retro_engine_pump_tasks(retro::Engine *engine,
                                           const std::int32_t max_tasks,
                                           retro::InteropError *error_message)
    {
        return retro::try_execute([&] { engine->pump_tasks(static_cast<std::size_t>(max_tasks)); }, *error_message);
    }

    RETRO_API void retro_engine_on_loop_exit(retro::Engine *engine)
    {
        engine->on_loop_exit();
    }

    RETRO_API bool retro_engine_render(retro::Engine *engine, retro::InteropError *error_message)
    {
        return retro::try_execute([&] { engine->render(); }, *error_message);
    }

    RETRO_API void retro_engine_wait_platform_events(retro::Engine *engine, std::int64_t timeout)
    {
        engine->wait_platform_event(std::chrono::milliseconds{timeout});
    }

    RETRO_API void retro_engine_poll_platform_events(retro::Engine *engine)
    {
        engine->poll_events_once();
    }

    RETRO_API void retro_engine_on_shutdown_requested_add(retro::Engine *engine,
                                                          void *user_data,
                                                          const ShutdownRequestedCallback removed_callback,
                                                          const DeleteCallback delete_callback,
                                                          const EqualsCallback equals_callback)
    {
        retro::InteropFunction<void()> function{removed_callback,
                                                std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                                equals_callback};
        engine->on_shutdown_requested().add(std::move(function));
    }

    RETRO_API void retro_engine_on_window_removed_add(retro::Engine *engine,
                                                      void *user_data,
                                                      const WindowRemovedCallback removed_callback,
                                                      const DeleteCallback delete_callback,
                                                      const EqualsCallback equals_callback)
    {
        retro::InteropFunction<void(std::uint64_t)> function{
            removed_callback,
            std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
            equals_callback};
        engine->on_window_removed().add(WindowRemovedBinding{std::move(function)});
    }

    RETRO_API void retro_engine_on_window_removed_remove(retro::Engine *engine,
                                                         void *user_data,
                                                         const WindowRemovedCallback removed_callback,
                                                         const DeleteCallback delete_callback,
                                                         const EqualsCallback equals_callback)
    {
        retro::InteropFunction<void(std::uint64_t)> function{
            removed_callback,
            std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
            equals_callback};
        engine->on_window_removed().remove(WindowRemovedBinding{std::move(function)});
    }
}
