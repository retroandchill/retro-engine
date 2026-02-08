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
#include "retro/runtime/assets/assets.h"

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif
    typedef struct Retro_Scene Retro_Scene;
    typedef struct Retro_Viewport Retro_Viewport;
    typedef struct Retro_Node Retro_Node;
    typedef struct Retro_Sprite Retro_Sprite;
    typedef struct Retro_Geometry Retro_Geometry;

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

    RETRO_API Retro_Scene *retro_scene_create();

    RETRO_API void retro_scene_destroy(Retro_Scene *scene);

    RETRO_API Retro_Viewport *retro_viewport_create();

    RETRO_API void retro_viewport_destroy(Retro_Viewport *viewport);

    RETRO_API void retro_viewport_set_scene(Retro_Viewport *viewport, Retro_Scene *scene);

    RETRO_API void retro_node_dispose(Retro_Scene *scene, Retro_Node *node);

    RETRO_API void retro_node_set_transform(Retro_Node *node, const Retro_Transform2f *transform);

    RETRO_API Retro_Geometry *retro_geometry_create(Retro_Scene *scene, Retro_Node *parent);

    RETRO_API void retro_geometry_set_type(Retro_Geometry *node, Retro_GeometryType type);

    RETRO_API void retro_geometry_set_render_data(Retro_Geometry *node,
                                                  const Retro_Vertex *vertices,
                                                  int32_t vertex_count,
                                                  uint32_t *indices,
                                                  int32_t index_count);

    RETRO_API void retro_geometry_set_color(Retro_Geometry *node, Retro_Color color);

    RETRO_API void retro_geometry_set_pivot(Retro_Geometry *node, Retro_Vector2f pivot);

    RETRO_API void retro_geometry_set_size(Retro_Geometry *node, Retro_Vector2f size);

    RETRO_API Retro_Sprite *retro_sprite_create(Retro_Scene *scene, Retro_Node *parent);

    RETRO_API void retro_sprite_set_texture(Retro_Sprite *node, Retro_Texture *texture);

    RETRO_API void retro_sprite_set_tint(Retro_Sprite *node, Retro_Color tint);

    RETRO_API void retro_sprite_set_pivot(Retro_Sprite *node, Retro_Vector2f pivot);

    RETRO_API void retro_sprite_set_size(Retro_Sprite *node, Retro_Vector2f size);

    static inline Retro_Node *retro_sprite_as_node(Retro_Sprite *sprite)
    {
        return (Retro_Node *)sprite;
    }

    static inline Retro_Node *retro_geometry_as_node(Retro_Geometry *geometry)
    {
        return (Retro_Node *)geometry;
    }

#ifdef __cplusplus
}
#endif
