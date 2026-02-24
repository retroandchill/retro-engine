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

namespace retro
{
    export enum class RenderBackend : std::uint8_t
    {
        vulkan
    };

    export RETRO_API void add_rendering_services(ServiceCollection &services,
                                                 WindowBackend window_backend,
                                                 RenderBackend backend = RenderBackend::vulkan);
} // namespace retro
