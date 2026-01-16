/**
 * @file quad.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"
#include "retro/core/math/color.h"
#include "retro/core/math/vector.h"
#include "retro/runtime/scene/scene.h"

#if __cplusplus
extern "C"
{
#endif

    RETRO_API void retro_quad_set_size(Retro_RenderObjectId id, Retro_Vector2f size);

    RETRO_API void retro_quad_set_color(Retro_RenderObjectId id, Retro_Color color);

#if __cplusplus
}
#endif
