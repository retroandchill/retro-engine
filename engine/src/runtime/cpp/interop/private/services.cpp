/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.renderer.services;
import retro.runtime.engine;
import retro.platform.window;

extern "C"
{

    RETRO_API void retro_add_rendering_services(retro::EngineConfigContext *config_context,
                                                const retro::WindowBackend window_backend,
                                                const retro::RenderBackendType render_backend)
    {
        retro::add_rendering_services(config_context->services, window_backend, render_backend);
    }
}
