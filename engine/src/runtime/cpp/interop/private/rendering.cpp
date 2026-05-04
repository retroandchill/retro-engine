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
import retro.runtime.rendering.texture;

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

    RETRO_API Texture *retro_render_backend_upload_texture(RenderBackend *backend,
                                                           const std::byte *bytes,
                                                           const std::int32_t length,
                                                           const std::int32_t width,
                                                           const std::int32_t height,
                                                           const TextureFormat format,
                                                           InteropError *error)
    {
        return try_execute(
            [&]
            {
                return backend
                    ->upload_texture(std::span{bytes, static_cast<std::size_t>(length)}, width, height, format)
                    .release();
            },
            *error);
    }

    RETRO_API bool retro_render_backend_export_texture(RenderBackend *backend,
                                                       const Texture *texture,
                                                       std::byte *buffer,
                                                       const std::int32_t length,
                                                       std::int32_t *bytes_written,
                                                       InteropError *error)
    {
        bool success = false;
        try_execute(
            [&]
            {
                auto [s, w] = backend->export_texture(*texture, std::span{buffer, static_cast<std::size_t>(length)});
                success = s;
                *bytes_written = static_cast<std::int32_t>(w);
            },
            *error);
        return success;
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
                                                         const std::int32_t pipeline_count,
                                                         const bool auto_assign_viewports,
                                                         InteropError *error)
    {
        return try_execute(
            [&]
            {
                return new RenderManager(
                    *platform_backend,
                    *render_backend,
                    *viewports,
                    PipelineManager{std::span{pipelines, static_cast<std::size_t>(pipeline_count)}},
                    auto_assign_viewports);
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
        std::uint64_t id = 0;
        const auto success = try_execute(
            [&]
            {
                return manager->create_new_window(WindowDesc{
                    .width = width,
                    .height = height,
                    .title = retro::convert_string<char>(
                        std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                    .flags = flags,
                });
            },
            [](const PlatformError &e)
            { return InteropError{.error_code = InteropErrorCode::platform_error, .message = e.message.data()}; },
            id,
            *error);

        if (!success)
        {
            return 0;
        }
        return id;
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
        try_execute_async(
            [manager,
             desc =
                 WindowDesc{
                     .width = width,
                     .height = height,
                     .title = retro::convert_string<char>(
                         std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                     .flags = flags,
                 }] mutable { return manager->create_new_window_async(std::move(desc)); },
            [](const PlatformError &e)
            { return InteropError{.error_code = InteropErrorCode::platform_error, .message = e.message.data()}; },
            [user_data, created_callback](std::uint64_t window_id) { created_callback(user_data, window_id); },
            [user_data, error_callback](const InteropError &error) { error_callback(user_data, error); });
    }

    RETRO_API std::uint64_t retro_render_create_window_from_handle(RenderManager *manager,
                                                                   const NativeWindowHandle handle,
                                                                   InteropError *error)
    {
        std::uint64_t id = 0;
        const auto success = try_execute(
            [&] { return manager->create_new_window(handle); },
            [](const PlatformError &e)
            { return InteropError{.error_code = InteropErrorCode::platform_error, .message = e.message.data()}; },
            id,
            *error);

        if (!success)
        {
            return 0;
        }
        return id;
    }

    RETRO_API void retro_render_manager_remove_window(RenderManager *manager,
                                                      const std::uint64_t window_id,
                                                      InteropError *error)
    {
        try_execute([&] { manager->remove_window(window_id); }, *error);
    }

    RETRO_API Window *retro_render_manager_get_window_by_id(const RenderManager *manager, const std::uint64_t window_id)
    {
        const auto result = manager->get_window(window_id);
        if (!result)
            return nullptr;
        return std::addressof(result.value());
    }

    RETRO_API void retro_render_manager_create_window_from_handle_async(RenderManager *manager,
                                                                        NativeWindowHandle handle,
                                                                        void *user_data,
                                                                        const WindowCreatedCallback created_callback,
                                                                        const OnErrorCallback error_callback)
    {
        try_execute_async(
            [manager, handle] { return manager->create_new_window_async(handle); },
            [](const PlatformError &e)
            { return InteropError{.error_code = InteropErrorCode::platform_error, .message = e.message.data()}; },
            [user_data, created_callback](const std::uint64_t window_id) { created_callback(user_data, window_id); },
            [user_data, error_callback](const InteropError &error) { error_callback(user_data, error); });
    }

    RETRO_API void retro_render_manager_set_viewport_window(const RenderManager *engine,
                                                            Viewport *viewport,
                                                            const std::uint64_t window_id)
    {
        engine->set_viewport_window(*viewport, window_id);
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
