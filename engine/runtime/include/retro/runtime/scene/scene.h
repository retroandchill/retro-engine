/**
 * @file scene.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#pragma once

#include "retro/core/exports.h"
#include "retro/core/math/color.h"
#include "retro/core/math/vector.h"
#include "retro/core/strings/name.h"

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif
    typedef uint32_t Retro_EntityId;

    typedef struct Retro_Transform
    {
        Retro_Vector2f position;
        float rotation;
        Retro_Vector2f scale;
    } Retro_Transform;

    typedef struct Retro_Vertex
    {
        Retro_Vector2f position{};
        Retro_Vector2f uv{};
        Retro_Color color{};
    } Retro_Vertex;

    RETRO_API Retro_EntityId retro_viewport_create(Retro_Vector2f viewport_size);

    RETRO_API void retro_entity_dispose(Retro_EntityId viewport_id);

    RETRO_API void retro_render_object_set_transform(Retro_EntityId render_object_id, const Retro_Transform *transform);

    RETRO_API void retro_geometry_set_render_data(Retro_EntityId render_object_id,
                                                  const Retro_Vertex *vertices,
                                                  int32_t vertex_count,
                                                  uint32_t *indices,
                                                  int32_t index_count);

#ifdef __cplusplus
}
#endif
