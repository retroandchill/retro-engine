/**
 * @file interop.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import retro.core.interop.interop_error;
import retro.runtime.rendering.render_backend;
import retro.platform.backend;
import retro.renderer.services;

using namespace retro;

extern "C"
{
    RETRO_API RenderBackend *retro_render_backend_create(PlatformBackend *platform_backend,
                                                         const RenderBackendType type,
                                                         InteropError *error)
    {
        return try_execute([&] { return create_render_backend(*platform_backend, type).release(); }, *error);
    }

    RETRO_API void retro_render_backend_destroy(const RenderBackend *backend)
    {
        backend->sub_ref();
    }
}
