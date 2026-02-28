/**
 * @file platform_backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.platform.backend;

extern "C"
{
    RETRO_API retro::PlatformBackend *retro_platform_backend_create(const retro::PlatformBackendInfo info,
                                                                    char *error_message,
                                                                    const std::int32_t error_message_length)
    {
        std::span error_message_span{error_message, static_cast<std::size_t>(error_message_length)};
        try
        {
            auto backend = retro::PlatformBackend::create(info);
            return backend.release();
        }
        catch (std::exception &e)
        {
            std::string_view error_message_view{e.what()};
            std::ranges::copy_n(
                error_message_view.begin(),
                static_cast<std::ptrdiff_t>(std::min(error_message_span.size(), error_message_view.size())),
                error_message_span.begin());
            return nullptr;
        }
        catch (...)
        {
            constexpr std::string_view error_message_view{"Unknown error"};
            std::ranges::copy_n(
                error_message_view.begin(),
                static_cast<std::ptrdiff_t>(std::min(error_message_span.size(), error_message_view.size())),
                error_message_span.begin());
            return nullptr;
        }
    }

    RETRO_API void retro_platform_backend_destroy(const retro::PlatformBackend *backend)
    {
        delete backend;
    }
}
