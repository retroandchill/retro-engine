/**
 * @file scene_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/runtime/scene/scene.h"

import retro.core;
import retro.runtime;
import std;

namespace
{
    Retro_NodeHandle to_c(retro::SceneNode *node)
    {
        return reinterpret_cast<std::uintptr_t>(node);
    }

    retro::SceneNode *from_c(const Retro_NodeHandle node) noexcept
    {
        return reinterpret_cast<retro::SceneNode *>(static_cast<std::uintptr_t>(node));
    }

    retro::Vector2f from_c(const Retro_Vector2f vector)
    {
        static_assert(sizeof(retro::Vector2f) == sizeof(Retro_Vector2f) &&
                      alignof(retro::Vector2f) == alignof(Retro_Vector2f));
        return std::bit_cast<retro::Vector2f>(vector);
    }

    retro::Color from_c(const Retro_Color vector)
    {
        static_assert(sizeof(retro::Color) == sizeof(Retro_Color) && alignof(retro::Color) == alignof(Retro_Color));
        return std::bit_cast<retro::Color>(vector);
    }

    const retro::Vertex *from_c(const Retro_Vertex *vertices)
    {
        static_assert(sizeof(Retro_Vertex) == sizeof(retro::Vertex));
        static_assert(alignof(Retro_Vertex) == alignof(retro::Vertex));
        return reinterpret_cast<const retro::Vertex *>(vertices);
    }

    retro::Transform2f from_c(const Retro_Transform2f &transform)
    {
        const retro::Scale2f scale{from_c(transform.scale)};
        const retro::Quaternion2f rotation{transform.rotation};

        const auto matrix = retro::Matrix2x2f{rotation} * retro::Matrix2x2f{scale};
        return retro::Transform2f{matrix, from_c(transform.position)};
    }

    retro::GeometryType from_c(const Retro_GeometryType type) noexcept
    {
        switch (type)
        {
            case Retro_GeometryType_Rectangle:
                return retro::GeometryType::Rectangle;
            case Retro_GeometryType_Triangle:
                return retro::GeometryType::Triangle;
            case Retro_GeometryType_Custom:
                return retro::GeometryType::Custom;
            default:
                return retro::GeometryType::None;
        }
    }
} // namespace

extern "C"
{

    void retro_node_dispose(const Retro_NodeHandle node)
    {
        retro::Engine::instance().scene().destroy_node(*from_c(node));
    }

    void retro_node_set_transform(const Retro_NodeHandle node, const Retro_Transform2f *transform)
    {
        auto *scene_node = from_c(node);
        scene_node->set_transform(from_c(*transform));
    }

    Retro_NodeHandle retro_viewport_create(const Retro_Vector2f viewport_size)
    {
        auto &vp = retro::Engine::instance().scene().create_viewport(from_c(viewport_size));
        return to_c(&vp);
    }

    void retro_scene_viewport_set_size(const Retro_NodeHandle node, const Retro_Vector2f viewport_size)
    {
        auto *base = from_c(node);
        auto &viewport = dynamic_cast<retro::ViewportNode &>(*base);
        viewport.viewport().view_size = from_c(viewport_size);
    }

    Retro_NodeHandle retro_geometry_create(const Retro_NodeHandle parent)
    {
        auto *parent_ptr = from_c(parent);
        auto &geo = retro::Engine::instance().scene().create_node<retro::GeometryObject>(parent_ptr);
        return to_c(&geo);
    }

    void retro_geometry_set_type(const Retro_NodeHandle node, const Retro_GeometryType type)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::GeometryObject &>(*base);
        geo.set_geometry(from_c(type));
    }

    void retro_geometry_set_render_data(const Retro_NodeHandle node,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::GeometryObject &>(*base);
        geo.set_geometry(std::make_shared<const retro::Geometry>(
            std::span{from_c(vertices), static_cast<usize>(vertex_count)} | std::ranges::to<std::vector>(),
            std::span{indices, static_cast<usize>(index_count)} | std::ranges::to<std::vector>()));
    }

    void retro_geometry_set_color(const Retro_NodeHandle node, const Retro_Color color)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::GeometryObject &>(*base);
        geo.set_color(from_c(color));
    }

    void retro_geometry_set_pivot(const Retro_NodeHandle node, const Retro_Vector2f pivot)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::GeometryObject &>(*base);
        geo.set_pivot(from_c(pivot));
    }

    void retro_geometry_set_size(const Retro_NodeHandle node, const Retro_Vector2f size)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::GeometryObject &>(*base);
        geo.set_size(from_c(size));
    }

    Retro_NodeHandle retro_sprite_create(const Retro_NodeHandle parent)
    {
        auto *parent_ptr = from_c(parent);
        auto &sprite = retro::Engine::instance().scene().create_node<retro::Sprite>(parent_ptr);
        return to_c(&sprite);
    }

    void retro_sprite_set_tint(const Retro_NodeHandle node, const Retro_Color tint)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::Sprite &>(*base);
        geo.set_tint(from_c(tint));
    }

    void retro_sprite_set_pivot(const Retro_NodeHandle node, const Retro_Vector2f pivot)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::Sprite &>(*base);
        geo.set_pivot(from_c(pivot));
    }

    void retro_sprite_set_size(const Retro_NodeHandle node, const Retro_Vector2f size)
    {
        auto *base = from_c(node);
        auto &geo = dynamic_cast<retro::Sprite &>(*base);
        geo.set_size(from_c(size));
    }
}
