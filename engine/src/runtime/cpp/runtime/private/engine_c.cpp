/**
 * @file engine_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.engine;

extern "C"
{

    RETRO_API void retro_engine_request_shutdown(const std::int32_t exit_code)
    {
        retro::Engine::instance().request_shutdown(exit_code);
    }
}
