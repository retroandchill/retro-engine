/**
 * @file backend_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/platform/backend_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.platform.backend;

DECLARE_DEFINED_C_HANDLE(Retro_PlatformBackendInfo, retro::PlatformBackendInfo)
DECLARE_OPAQUE_C_HANDLE(Retro_PlatformBackend, retro::PlatformBackend)

Retro_PlatformBackend *retro_platform_backend_create(RetroPlatformBackendInfo info,
                                                     char *error_message,
                                                     const int32_t error_message_length)
{
    std::span error_message_span{error_message, static_cast<std::size_t>(error_message_length)};
    try
    {
        auto backend = retro::PlatformBackend::create(retro::from_c(info));
        return retro::to_c(backend.release());
    }
    catch (std::exception &e)
    {
        std::string_view error_message_view{e.what()};
        std::ranges::copy_n(error_message_view.begin(),
                            static_cast<std::ptrdiff_t>(std::min(error_message_span.size(), error_message_view.size())),
                            error_message_span.begin());
        return nullptr;
    }
    catch (...)
    {
        constexpr std::string_view error_message_view{"Unknown error"};
        std::ranges::copy_n(error_message_view.begin(),
                            static_cast<std::ptrdiff_t>(std::min(error_message_span.size(), error_message_view.size())),
                            error_message_span.begin());
        return nullptr;
    }
}

void retro_platform_backend_destroy(Retro_PlatformBackend *backend)
{
    delete retro::from_c(backend);
}
