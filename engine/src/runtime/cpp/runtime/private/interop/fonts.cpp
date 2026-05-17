/**
 * @file fonts.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.rendering.text.font_service;
import retro.core.interop.interop_error;

using namespace retro;

extern "C"
{
    RETRO_API FontService *retro_font_service_create(InteropError *error)
    {
        return try_execute([] { return new FontService(); }, *error);
    }

    RETRO_API void retro_font_service_destroy(const FontService *service)
    {
        delete service;
    }
}
