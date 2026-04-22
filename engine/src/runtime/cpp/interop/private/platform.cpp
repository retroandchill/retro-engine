/**
 * @file platform.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import retro.platform.backend;
import retro.interop.interop_error;
import retro.platform.window;

using namespace retro;

extern "C"
{
    RETRO_API PlatformBackend *retro_platform_backend_create(const PlatformBackendKind kind,
                                                             const PlatformInitFlags flags,
                                                             InteropError *error)
    {
        return try_execute([&] { return PlatformBackend::create({kind, flags}).release(); }, *error);
    }

    RETRO_API void retro_platform_backend_destroy(const PlatformBackend *backend)
    {
        delete backend;
    }

    RETRO_API void retro_window_add_ref(const Window *window)
    {
        window->add_ref();
    }

    RETRO_API void retro_window_sub_ref(const Window *window)
    {
        window->sub_ref();
    }
}
