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
    typedef uint64_t Retro_NodeHandle;

    typedef struct Retro_Vertex
    {
        Retro_Vector2f position{};
        Retro_Vector2f uv{};
        Retro_Color color{};
    } Retro_Vertex;

    typedef struct Retro_Transform2f
    {
        Retro_Vector2f position;
        float rotation;
        Retro_Vector2f scale;
    } Retro_Transform2f;

    typedef struct Retro_ViewUpdate
    {
        Retro_NodeHandle node;
        Retro_Vector2f viewport_size;
    } Retro_ViewUpdate;

    RETRO_API void retro_node_dispose(Retro_NodeHandle node);

    RETRO_API void retro_node_set_transform(Retro_NodeHandle node, const Retro_Transform2f *transform);

    RETRO_API Retro_NodeHandle retro_viewport_create(Retro_Vector2f viewport_size);

    RETRO_API void retro_scene_viewport_set_size(Retro_NodeHandle node, Retro_Vector2f viewport_size);

    RETRO_API Retro_NodeHandle retro_geometry_create(Retro_NodeHandle parent);

    RETRO_API void retro_geometry_set_render_data(Retro_NodeHandle node,
                                                  const Retro_Vertex *vertices,
                                                  int32_t vertex_count,
                                                  uint32_t *indices,
                                                  int32_t index_count);

#ifdef __cplusplus
}
#endif
