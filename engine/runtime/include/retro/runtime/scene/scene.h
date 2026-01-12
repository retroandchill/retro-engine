/**
 * @file scene.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#pragma once

#include "retro/core/exports.h"
#include "retro/core/math/vector.h"
#include "retro/core/strings/name.h"

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif
    typedef struct Retro_DefaultHandle
    {
        uint32_t index;
        uint32_t generation;
    } Retro_DefaultHandle;

    typedef struct Retro_Transform
    {
        Retro_Vector2f position;
        float rotation;
        Retro_Vector2f scale;
    } Retro_Transform;

    typedef Retro_DefaultHandle Retro_ViewportId;

    typedef Retro_DefaultHandle Retro_RenderObjectId;

    RETRO_API Retro_ViewportId retro_viewport_create();

    RETRO_API void retro_viewport_dispose(Retro_ViewportId viewport_id);

    RETRO_API Retro_RenderObjectId retro_render_object_create(Retro_Name name,
                                                              Retro_ViewportId viewport_id,
                                                              const void *payload,
                                                              int32_t size);

    RETRO_API void retro_render_object_dispose(Retro_RenderObjectId render_object_id);

    RETRO_API void retro_render_object_set_transform(Retro_RenderObjectId render_object_id,
                                                     const Retro_Transform *transform);

#ifdef __cplusplus
}
#endif
