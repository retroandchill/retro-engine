/**
 * @file engine_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

#include "retro/runtime/engine_c.h"

import retro.runtime.engine;

extern "C"
{

    void retro_engine_request_shutdown(const int32_t exit_code)
    {
        retro::Engine::instance().request_shutdown(exit_code);
    }
}
