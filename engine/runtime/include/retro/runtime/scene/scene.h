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

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif
    typedef uintptr_t Retro_NodeHandle;

    typedef struct Retro_Vertex
    {
        Retro_Vector2f position{};
        Retro_Vector2f uv{};
    } Retro_Vertex;

    typedef struct Retro_Transform2f
    {
        Retro_Vector2f position;
        float rotation;
        Retro_Vector2f scale;
    } Retro_Transform2f;

    enum Retro_GeometryTypeEnum
    {
        Retro_GeometryType_None,
        Retro_GeometryType_Rectangle,
        Retro_GeometryType_Triangle,
        Retro_GeometryType_Custom
    };

    typedef uint8_t Retro_GeometryType;

    RETRO_API void retro_node_dispose(Retro_NodeHandle node);

    RETRO_API void retro_node_set_transform(Retro_NodeHandle node, const Retro_Transform2f *transform);

    RETRO_API Retro_NodeHandle retro_viewport_create(Retro_Vector2f viewport_size);

    RETRO_API void retro_scene_viewport_set_size(Retro_NodeHandle node, Retro_Vector2f viewport_size);

    RETRO_API Retro_NodeHandle retro_geometry_create(Retro_NodeHandle parent);

    RETRO_API void retro_geometry_set_type(Retro_NodeHandle node, Retro_GeometryType type);

    RETRO_API void retro_geometry_set_render_data(Retro_NodeHandle node,
                                                  const Retro_Vertex *vertices,
                                                  int32_t vertex_count,
                                                  uint32_t *indices,
                                                  int32_t index_count);

    RETRO_API void retro_geometry_set_color(Retro_NodeHandle node, Retro_Color color);

    RETRO_API void retro_geometry_set_pivot(Retro_NodeHandle node, Retro_Vector2f pivot);

    RETRO_API void retro_geometry_set_size(Retro_NodeHandle node, Retro_Vector2f size);

    RETRO_API Retro_NodeHandle retro_sprite_create(Retro_NodeHandle parent);

    RETRO_API void retro_sprite_set_tint(Retro_NodeHandle node, Retro_Color tint);

    RETRO_API void retro_sprite_set_pivot(Retro_NodeHandle node, Retro_Vector2f pivot);

    RETRO_API void retro_sprite_set_size(Retro_NodeHandle node, Retro_Vector2f size);

#ifdef __cplusplus
}
#endif
