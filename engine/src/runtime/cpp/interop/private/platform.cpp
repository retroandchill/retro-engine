/**
 * @file platform.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import retro.platform.backend;
import retro.interop.interop_error;

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
}
