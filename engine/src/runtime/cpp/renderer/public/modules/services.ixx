/**
 * @file services.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer.services;

import std;
import retro.core.di;
import retro.platform.window;
import retro.runtime.rendering.render_backend;
import retro.core.memory.ref_counted_ptr;
import retro.platform.backend;

namespace retro
{
    export enum class RenderBackendType : std::uint8_t
    {
        headless,
        vulkan
    };

    export RETRO_API RefCountPtr<RenderBackend> create_render_backend(PlatformBackend &platform_backend,
                                                                      RenderBackendType backend);

    export RETRO_API void add_rendering_services(ServiceCollection &services,
                                                 WindowBackend window_backend,
                                                 RenderBackendType backend = RenderBackendType::vulkan);
} // namespace retro
