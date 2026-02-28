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
import retro.platform.backend;
import retro.platform.window;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.filesystem_asset_source;
import retro.runtime.assets.textures.texture_decoder;

namespace
{
    using ConfigCallback = void (*)(retro::EngineConfigContext *, void *);

    using StartCallback = std::int32_t (*)(void *);
    using UpdateCallback = void (*)(void *, float);
    using StopCallback = void (*)(void *);

    using DeleteCallback = void (*)(void *);
    using EqualsCallback = bool (*)(void *, void *);
    using WindowRemovedCallback = void (*)(void *, std::uint64_t);

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
                                                 char *error_message,
                                                 std::int32_t error_message_length)
    {
        try
        {
            retro::EngineConfigContext context;
            context.services.add(retro::PlatformBackend::create(platform_config));
            context.services.add_singleton<retro::PipelineManager>()
                .add_singleton<retro::RenderPipeline, retro::GeometryRenderPipeline>()
                .add_singleton<retro::RenderPipeline, retro::SpriteRenderPipeline>()
                .add_singleton<retro::AssetSource, retro::FileSystemAssetSource>()
                .add_singleton<retro::AssetManager>()
                .add_singleton<retro::AssetDecoder, retro::TextureDecoder>();

            if (config_callback != nullptr)
            {
                config_callback(std::addressof(context), user_data);
            }

            auto engine = std::make_unique<retro::Engine>(context.services.create_service_provider());
            retro::Engine::initialize(*engine);
            return engine.release();
        }
        catch (const std::exception &e)
        {
            std::string_view error_message_view{e.what()};
            std::ranges::copy_n(error_message_view.begin(),
                                std::min(error_message_length, static_cast<std::int32_t>(error_message_view.size())),
                                error_message);
            return nullptr;
        }
        catch (...)
        {
            std::string_view error_message_view{"Unknown error"};
            std::ranges::copy_n(error_message_view.begin(),
                                std::min(error_message_length, static_cast<std::int32_t>(error_message_view.size())),
                                error_message);
            return nullptr;
        }
    }

    RETRO_API std::uint64_t retro_engine_create_main_window(retro::Engine *engine,
                                                            const char *window_title,
                                                            std::int32_t width,
                                                            std::int32_t height,
                                                            retro::WindowFlags flags,
                                                            char *error_message,
                                                            std::int32_t error_message_length)
    {
        try
        {
            auto &window = engine->create_new_window(
                retro::WindowDesc{.width = width, .height = height, .title = window_title, .flags = flags});
            return window.id();
        }
        catch (const std::exception &e)
        {
            std::string_view error_message_view{e.what()};
            std::ranges::copy_n(error_message_view.begin(),
                                std::min(error_message_length, static_cast<std::int32_t>(error_message_view.size())),
                                error_message);
            return 0;
        }
        catch (...)
        {
            std::string_view error_message_view{"Unknown error"};
            std::ranges::copy_n(error_message_view.begin(),
                                std::min(error_message_length, static_cast<std::int32_t>(error_message_view.size())),
                                error_message);
            return 0;
        }
    }

    RETRO_API void retro_destroy_engine(retro::Engine *engine)
    {
        // Put the engine back into a unique_ptr so no matter what happens here it always get destroyed
        std::unique_ptr<retro::Engine> ptr(engine);
        retro::Engine::shutdown();
    }

    RETRO_API void retro_engine_run(retro::Engine *engine,
                                    void *user_data,
                                    StartCallback start_callback,
                                    UpdateCallback update_callback,
                                    StopCallback stop_callback)
    {
        auto bound_start_callback = std::bind_front(start_callback, user_data);
        auto bound_update_callback = std::bind_front(update_callback, user_data);
        auto bound_stop_callback = std::bind_front(stop_callback, user_data);
        engine->run(retro::EngineCallbacks{.start = bound_start_callback,
                                           .tick = bound_update_callback,
                                           .stop = bound_stop_callback});
    }

    RETRO_API void retro_engine_poll_platform_events(retro::Engine *engine)
    {
        engine->run_platform_event_loop();
    }

    RETRO_API void retro_engine_request_shutdown(retro::Engine *engine, const std::int32_t exit_code)
    {
        engine->request_shutdown(exit_code);
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
