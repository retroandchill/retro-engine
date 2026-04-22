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
import retro.runtime.rendering.pipeline_manager.renderer_manager;
import retro.runtime.world.viewport;
import retro.runtime.rendering.pipeline_manager;
import retro.platform.window;
import retro.core.strings.encoding;
import retro.core.async.task;

using namespace retro;

using WindowCreatedCallback = void (*)(void *, std::uint64_t);
using OnErrorCallback = void (*)(void *, InteropError);

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

    RETRO_API RendererManager *retro_renderer_manager_create(PlatformBackend *platform_backend,
                                                             RenderBackend *render_backend,
                                                             ViewportManager *viewports,
                                                             RenderPipeline **pipelines,
                                                             std::int32_t pipeline_count,
                                                             InteropError *error)
    {
        return try_execute(
            [&]
            {
                return new RendererManager(
                    *platform_backend,
                    *render_backend,
                    *viewports,
                    PipelineManager{std::span{pipelines, static_cast<std::size_t>(pipeline_count)}});
            },
            *error);
    }

    RETRO_API void retro_renderer_manager_destroy(const RendererManager *manager)
    {
        delete manager;
    }

    RETRO_API std::uint64_t retro_renderer_manager_create_window(RendererManager *manager,
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

            return window->get()->id();
        }
        catch (const std::exception &e)
        {
            *error = InteropError{.error_code = get_error_code(e),
                                  .native_exception_type = typeid(e).name(),
                                  .message = e.what()};
        }
        catch (...)
        {
            *error = InteropError{.error_code = InteropErrorCode::unknown, .message = "Unknown error"};
        }

        return 0;
    }

    RETRO_API void retro_renderer_manager_create_window_async(RendererManager *manager,
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
                         RendererManager *local_manager,
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

                on_created(local_user_data, window->get()->id());
            }
            catch (const std::exception &e)
            {
                on_error(local_user_data,
                         InteropError{.error_code = get_error_code(e),
                                      .native_exception_type = typeid(e).name(),
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
}
