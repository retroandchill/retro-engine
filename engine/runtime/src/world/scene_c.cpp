/**
 * @file scene_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/macros.hpp"
#include "retro/runtime/scene/scene.h"

import retro.core.util.color;
import retro.core.math.vector;
import retro.core.math.matrix;
import retro.core.math.transform;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.world.scene;
import retro.runtime.world.viewport;
import retro.runtime.world.scene_node;
import retro.runtime.assets.textures.texture;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.engine;
import retro.core.c_api;
import std;

DECLARE_OPAQUE_C_HANDLE(Retro_Scene, retro::Scene);
DECLARE_OPAQUE_C_HANDLE(Retro_Viewport, retro::Viewport);
DECLARE_OPAQUE_C_HANDLE(Retro_Node, retro::SceneNode);
DECLARE_OPAQUE_C_HANDLE(Retro_Sprite, retro::Sprite);
DECLARE_OPAQUE_C_HANDLE(Retro_Geometry, retro::GeometryObject);
DECLARE_OPAQUE_C_HANDLE(Retro_Texture, retro::Texture);
DECLARE_DEFINED_C_HANDLE(Retro_Vector2f, retro::Vector2f);
DECLARE_DEFINED_C_HANDLE(Retro_Color, retro::Color);
DECLARE_DEFINED_C_HANDLE(Retro_Vertex, retro::Vertex);
DECLARE_DEFINED_C_HANDLE(Retro_ScreenLayout, retro::ScreenLayout);
DECLARE_DEFINED_C_HANDLE(Retro_UVs, retro::UVs);

using retro::from_c;
using retro::to_c;

namespace
{
    retro::Transform2f from_c(const Retro_Transform2f &transform)
    {
        const retro::Scale2f scale{from_c(transform.scale)};
        const retro::Quaternion2f rotation{transform.rotation};

        const auto matrix = retro::Matrix2x2f{rotation} * retro::Matrix2x2f{scale};
        return retro::Transform2f{matrix, from_c(transform.position)};
    }

    retro::CameraLayout from_c(const Retro_CameraLayout &layout)
    {
        return retro::CameraLayout{.position = from_c(layout.position),
                                   .pivot = from_c(layout.pivot),
                                   .rotation = retro::Quaternion2f{layout.rotation},
                                   .zoom = layout.zoom};
    }

    retro::GeometryType from_c(const Retro_GeometryType type) noexcept
    {
        switch (type)
        {
            case Retro_GeometryType_Rectangle:
                return retro::GeometryType::rectangle;
            case Retro_GeometryType_Triangle:
                return retro::GeometryType::triangle;
            case Retro_GeometryType_Custom:
                return retro::GeometryType::custom;
            default:
                return retro::GeometryType::none;
        }
    }
} // namespace

extern "C"
{
    Retro_Scene *retro_scene_create()
    {
        return std::addressof(to_c(retro::Engine::instance().scenes().create_scene()));
    }

    void retro_scene_destroy(Retro_Scene *scene)
    {
        retro::Engine::instance().scenes().destroy_scene(*from_c(scene));
    }

    Retro_Viewport *retro_viewport_create()
    {
        return std::addressof(to_c(retro::Engine::instance().viewports().create_viewport()));
    }

    void retro_viewport_destroy(Retro_Viewport *viewport)
    {
        retro::Engine::instance().viewports().destroy_viewport(*from_c(viewport));
    }

    void retro_viewport_set_scene(Retro_Viewport *viewport, Retro_Scene *scene)
    {
        from_c(viewport)->set_scene(from_c(scene));
    }

    void retro_viewport_set_screen_layout(Retro_Viewport *viewport, const Retro_ScreenLayout *layout)
    {
        from_c(viewport)->set_screen_layout(from_c(*layout));
    }

    void retro_viewport_set_camera_layout(Retro_Viewport *viewport, const Retro_CameraLayout *layout)
    {
        from_c(viewport)->set_camera_layout(from_c(*layout));
    }

    void retro_node_dispose(Retro_Scene *scene, Retro_Node *node)
    {
        from_c(scene)->destroy_node(*from_c(node));
    }

    void retro_node_set_transform(Retro_Node *node, const Retro_Transform2f *transform)
    {
        auto *scene_node = from_c(node);
        scene_node->set_transform(from_c(*transform));
    }

    Retro_Geometry *retro_geometry_create(Retro_Scene *scene, Retro_Node *parent)
    {
        auto *parent_ptr = from_c(parent);
        auto &geo = from_c(scene)->create_node<retro::GeometryObject>(parent_ptr);
        return to_c(&geo);
    }

    void retro_geometry_set_type(Retro_Geometry *node, const Retro_GeometryType type)
    {
        auto &geo = *from_c(node);
        geo.set_geometry(from_c(type));
    }

    void retro_geometry_set_render_data(Retro_Geometry *node,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto &geo = *from_c(node);
        geo.set_geometry(std::make_shared<const retro::Geometry>(
            std::span{from_c(vertices), static_cast<std::size_t>(vertex_count)} | std::ranges::to<std::vector>(),
            std::span{indices, static_cast<std::size_t>(index_count)} | std::ranges::to<std::vector>()));
    }

    void retro_geometry_set_color(Retro_Geometry *node, const Retro_Color color)
    {
        auto &geo = *from_c(node);
        geo.set_color(from_c(color));
    }

    void retro_geometry_set_pivot(Retro_Geometry *node, const Retro_Vector2f pivot)
    {
        auto &geo = *from_c(node);
        geo.set_pivot(from_c(pivot));
    }

    void retro_geometry_set_size(Retro_Geometry *node, const Retro_Vector2f size)
    {
        auto &geo = *from_c(node);
        geo.set_size(from_c(size));
    }

    Retro_Sprite *retro_sprite_create(Retro_Scene *scene, Retro_Node *parent)
    {
        auto *parent_ptr = from_c(parent);
        auto &sprite = from_c(scene)->create_node<retro::Sprite>(parent_ptr);
        return to_c(&sprite);
    }

    void retro_sprite_set_texture(Retro_Sprite *node, Retro_Texture *texture)
    {
        auto &sprite = *from_c(node);
        sprite.set_texture(retro::RefCountPtr(from_c(texture)));
    }

    void retro_sprite_set_tint(Retro_Sprite *node, const Retro_Color tint)
    {
        auto &geo = *from_c(node);
        geo.set_tint(from_c(tint));
    }

    void retro_sprite_set_pivot(Retro_Sprite *node, const Retro_Vector2f pivot)
    {
        auto &geo = *from_c(node);
        geo.set_pivot(from_c(pivot));
    }

    void retro_sprite_set_size(Retro_Sprite *node, const Retro_Vector2f size)
    {
        auto &geo = *from_c(node);
        geo.set_size(from_c(size));
    }

    void retro_sprite_set_uv_rect(Retro_Sprite *node, Retro_UVs uv_rect)
    {
        auto &sprite = *from_c(node);
        sprite.set_uvs(from_c(uv_rect));
    }
}
