/**
 * @file rendering.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.rendering.render_backend;
import retro.renderer.services;
import retro.interop.interop_error;
import retro.platform.backend;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.rendering.pipeline_manager.render_manager;
import retro.runtime.world.viewport;
import retro.runtime.rendering.pipeline_manager;
import retro.platform.window;
import retro.core.strings.encoding;
import retro.core.async.task;
import retro.core.functional.interop_function;

using namespace retro;

namespace
{
    struct WindowRemovedBinding
    {
        InteropFunction<void(std::uint64_t)> function;

        void operator()(const Window &window) const
        {
            function(window.id());
        }

        bool operator==(const WindowRemovedBinding &other) const noexcept
        {
            return function == other.function;
        }
    };

    using DeleteCallback = void (*)(void *);
    using EqualsCallback = bool (*)(void *, void *);
    using WindowRemovedCallback = void (*)(void *, std::uint64_t);

    using WindowCreatedCallback = void (*)(void *, std::uint64_t);
    using OnErrorCallback = void (*)(void *, InteropError);
} // namespace

extern "C"
{
    RETRO_API RenderBackend *retro_render_backend_create(PlatformBackend *platform_backend,
                                                         RenderBackendType type,
                                                         InteropError *error)
    {
        return try_execute([&] { return create_render_backend(*platform_backend, type).release(); }, *error);
    }

    RETRO_API void retro_render_backend_destroy(const RenderBackend *backend)
    {
        backend->sub_ref();
    }

    RETRO_API void retro_render_pipeline_destroy(const RenderPipeline *pipeline)
    {
        delete pipeline;
    }

    RETRO_API RenderPipeline *retro_render_pipeline_create_geometry(InteropError *error)
    {
        return try_execute([] { return new GeometryRenderPipeline(); }, *error);
    }

    RETRO_API RenderPipeline *retro_render_pipeline_create_sprite(InteropError *error)
    {
        return try_execute([] { return new SpriteRenderPipeline(); }, *error);
    }

    RETRO_API RenderManager *retro_render_manager_create(PlatformBackend *platform_backend,
                                                         RenderBackend *render_backend,
                                                         ViewportManager *viewports,
                                                         RenderPipeline **pipelines,
                                                         std::int32_t pipeline_count,
                                                         InteropError *error)
    {
        return try_execute(
            [&]
            {
                return new RenderManager(
                    *platform_backend,
                    *render_backend,
                    *viewports,
                    PipelineManager{std::span{pipelines, static_cast<std::size_t>(pipeline_count)}});
            },
            *error);
    }

    RETRO_API void retro_render_manager_destroy(const RenderManager *manager)
    {
        delete manager;
    }

    RETRO_API std::uint64_t retro_render_manager_create_window(RenderManager *manager,
                                                               const char16_t *window_title,
                                                               const std::int32_t window_tile_length,
                                                               const std::int32_t width,
                                                               const std::int32_t height,
                                                               const WindowFlags flags,
                                                               InteropError *error)
    {
        try
        {
            auto window = manager->create_new_window(WindowDesc{
                .width = width,
                .height = height,
                .title = retro::convert_string<char>(
                    std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                .flags = flags,
            });
            if (!window.has_value())
            {
                *error = InteropError{.error_code = InteropErrorCode::platform_error,
                                      .message = window.error().message.data()};
                return 0;
            }

            return *window;
        }
        catch (const std::exception &e)
        {
            auto &type_id = typeid(e);
            *error = InteropError{.error_code = get_error_code(e),
                                  .native_exception_type = type_id.name(),
                                  .message = e.what()};
        }
        catch (...)
        {
            *error = InteropError{.error_code = InteropErrorCode::unknown, .message = "Unknown error"};
        }

        return 0;
    }

    RETRO_API void retro_render_manager_create_window_async(RenderManager *manager,
                                                            const char16_t *window_title,
                                                            const std::int32_t window_tile_length,
                                                            const std::int32_t width,
                                                            const std::int32_t height,
                                                            const WindowFlags flags,
                                                            void *user_data,
                                                            const WindowCreatedCallback created_callback,
                                                            const OnErrorCallback error_callback)
    {
        std::ignore = [](WindowDesc &&desc,
                         RenderManager *local_manager,
                         void *local_user_data,
                         const WindowCreatedCallback on_created,
                         const OnErrorCallback on_error) -> Task<>
        {
            try
            {
                auto window = co_await local_manager->create_new_window_async(std::move(desc));
                if (!window.has_value())
                {
                    on_error(local_user_data,
                             InteropError{.error_code = InteropErrorCode::platform_error,
                                          .message = window.error().message.data()});
                }

                on_created(local_user_data, *window);
            }
            catch (const std::exception &e)
            {
                auto &type_id = typeid(e);
                on_error(local_user_data,
                         InteropError{.error_code = get_error_code(e),
                                      .native_exception_type = type_id.name(),
                                      .message = e.what()});
            }
            catch (...)
            {
                on_error(local_user_data,
                         InteropError{.error_code = InteropErrorCode::unknown, .message = "Unknown error"});
            }
        }(
            WindowDesc{
                .width = width,
                .height = height,
                .title = retro::convert_string<char>(
                    std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                .flags = flags,
            },
            manager,
            user_data,
            created_callback,
            error_callback);
    }

    RETRO_API void retro_render_manager_on_window_removed_add(RenderManager *engine,
                                                              void *user_data,
                                                              const WindowRemovedCallback removed_callback,
                                                              const DeleteCallback delete_callback,
                                                              const EqualsCallback equals_callback)
    {
        InteropFunction<void(std::uint64_t)> function{removed_callback,
                                                      std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                                      equals_callback};
        engine->on_window_removed().add(WindowRemovedBinding{std::move(function)});
    }

    RETRO_API void retro_render_manager_on_window_removed_remove(RenderManager *engine,
                                                                 void *user_data,
                                                                 const WindowRemovedCallback removed_callback,
                                                                 const DeleteCallback delete_callback,
                                                                 const EqualsCallback equals_callback)
    {
        InteropFunction<void(std::uint64_t)> function{removed_callback,
                                                      std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                                      equals_callback};
        engine->on_window_removed().remove(WindowRemovedBinding{std::move(function)});
    }

    RETRO_API void retro_render_manager_sync_render_state(RenderManager *engine, InteropError *error)
    {
        try_execute([&] { engine->sync_renderer_state(); }, *error);
    }

    RETRO_API void retro_render_manager_render(const RenderManager *engine, InteropError *error_message)
    {
        try_execute([&] { engine->render(); }, *error_message);
    }

    RETRO_API void retro_render_manager_on_engine_shutdown(RenderManager *engine)
    {
        engine->on_engine_shutdown();
    }
}
