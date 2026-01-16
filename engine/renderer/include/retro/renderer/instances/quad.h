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

    typedef struct Retro_QuadUpdateData
    {
        Retro_Vector2f size;
        Retro_Color color;
    } Retro_QuadUpdateData;

    RETRO_API void retro_quad_update_data(Retro_RenderObjectId id, const Retro_QuadUpdateData *color);

#if __cplusplus
}
#endif
