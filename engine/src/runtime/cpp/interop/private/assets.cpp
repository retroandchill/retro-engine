/**
 * @file assets.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <boost/pool/pool_alloc.hpp>

import retro.runtime.engine;
import retro.core.strings.name;
import retro.core.math.vector;
import std;
import retro.runtime.rendering.texture;

namespace retro
{
    struct CVector2i
    {
        std::int32_t x = 0;
        std::int32_t y = 0;
    };
} // namespace retro

extern "C"
{
    RETRO_API void retro_texture_destroy(retro::Texture *texture)
    {
        texture->sub_ref();
    }

    RETRO_API retro::CVector2i retro_texture_get_size(const retro::Texture *texture)
    {
        return retro::CVector2i{
            .x = texture->width(),
            .y = texture->height(),
        };
    }
}
