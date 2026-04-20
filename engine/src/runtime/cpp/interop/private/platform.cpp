/**
 * @file platform.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.platform.backend;
import retro.interop.interop_error;
import retro.platform.window;
import retro.core.strings.encoding;
import retro.core.async.task;

namespace
{
    using WindowCreatedCallback = void (*)(void *, retro::Window *);
    using OnErrorCallback = void (*)(void *, retro::InteropError);
} // namespace

extern "C"
{
    RETRO_API retro::PlatformBackend *retro_platform_backend_create(const retro::PlatformBackendKind kind,
                                                                    const retro::PlatformInitFlags flags,
                                                                    retro::InteropError *error_message)
    {
        return retro::try_execute(
            [&]
            {
                return retro::PlatformBackend::create({
                                                          .kind = kind,
                                                          .flags = flags,
                                                      })
                    .release();
            },
            *error_message);
    }

    RETRO_API void retro_platform_backend_destroy(const retro::PlatformBackend *backend)
    {
        delete backend;
    }

    RETRO_API retro::Window *retro_platform_backend_create_window(retro::PlatformBackend *backend,
                                                                  const char16_t *window_title,
                                                                  const std::int32_t window_tile_length,
                                                                  const std::int32_t width,
                                                                  const std::int32_t height,
                                                                  const retro::WindowFlags flags,
                                                                  retro::InteropError *error_message)
    {
        auto result = backend->create_window(retro::WindowDesc{
            .width = width,
            .height = height,
            .title = retro::convert_string<char>(
                std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
            .flags = flags,
        });
        if (!result.has_value())
        {
            *error_message = retro::InteropError{
                .error_code = retro::InteropErrorCode::platform_error,
                .message = result.error().message.data(),
            };
            return nullptr;
        }

        return result->release();
    }

    RETRO_API retro::Window *retro_platform_backend_create_window_async(retro::PlatformBackend *backend,
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
                         retro::PlatformBackend *local_backend,
                         void *local_user_data,
                         const WindowCreatedCallback on_created,
                         const OnErrorCallback on_error) -> retro::Task<>
        {
            try
            {
                auto window = co_await local_backend->create_window_async(std::move(desc));
                if (!window.has_value())
                {
                    on_error(local_user_data,
                             retro::InteropError{
                                 .error_code = retro::InteropErrorCode::platform_error,
                                 .message = window.error().message.data(),
                             });
                }

                on_created(local_user_data, window.value().release());
            }
            catch (const std::exception &e)
            {
                on_error(local_user_data,
                         retro::InteropError{
                             .error_code = retro::get_error_code(e),
                             .native_exception_type = typeid(e).name(),
                             .message = e.what(),
                         });
            }
            catch (...)
            {
                on_error(local_user_data,
                         retro::InteropError{
                             .error_code = retro::InteropErrorCode::unknown,
                             .message = "Unknown error",
                         });
            }
        }(
            retro::WindowDesc{
                .width = width,
                .height = height,
                .title = retro::convert_string<char>(
                    std::u16string_view{window_title, static_cast<std::size_t>(window_tile_length)}),
                .flags = flags,
            },
            backend,
            user_data,
            created_callback,
            error_callback);
    }
}
