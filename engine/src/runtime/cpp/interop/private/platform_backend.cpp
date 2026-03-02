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
                                                                    const char **error_message)
    {
        try
        {
            auto backend = retro::PlatformBackend::create(info);
            return backend.release();
        }
        catch (std::exception &e)
        {
            *error_message = e.what();
            return nullptr;
        }
        catch (...)
        {
            *error_message = "Unknown error";
            return nullptr;
        }
    }

    RETRO_API void retro_platform_backend_destroy(const retro::PlatformBackend *backend)
    {
        delete backend;
    }
}
