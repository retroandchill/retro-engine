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

    const retro::Vertex *from_c(const Retro_Vertex *vertices)
    {
        static_assert(sizeof(Retro_Vertex) == sizeof(retro::Vertex));
        static_assert(alignof(Retro_Vertex) == alignof(retro::Vertex));
        return reinterpret_cast<const retro::Vertex *>(vertices);
    }
} // namespace

extern "C"
{

    void retro_node_dispose(const Retro_NodeHandle node)
    {
        retro::Engine::instance().scene().destroy_node(*from_c(node));
    }

    void retro_scene_update_transforms(const Retro_TransformUpdate *updates, const int32_t update_count)
    {
        std::span update_span{updates, static_cast<usize>(update_count)};
        for (const auto &[node, position, rotation, scale] : update_span)
        {
            auto *ptr = from_c(node);
            if (ptr == nullptr)
            {
                continue;
            }

            auto &transform = ptr->transform();
            transform.set_position(from_c(position));
            transform.set_rotation(rotation);
            transform.set_scale(from_c(scale));
        }
    }

    Retro_NodeHandle retro_viewport_create(const Retro_Vector2f viewport_size)
    {
        auto &vp = retro::Engine::instance().scene().create_viewport(from_c(viewport_size));
        return to_c(&vp);
    }

    void retro_scene_update_viewports(const Retro_ViewUpdate *updates, const int32_t update_count)
    {
        for (auto [node, viewport_size] : std::span{updates, static_cast<usize>(update_count)})
        {
            auto *base = from_c(node);
            if (base == nullptr)
            {
                continue;
            }

            // Caller must pass a viewport node handle here.
            auto *vp = dynamic_cast<retro::ViewportNode *>(base);
            if (vp == nullptr)
            {
                continue;
            }

            vp->viewport().view_size = from_c(viewport_size);
        }
    }

    Retro_NodeHandle retro_geometry_create(Retro_NodeHandle parent)
    {
        auto *parent_ptr = from_c(parent);

        auto &geo = retro::Engine::instance().scene().create_node<retro::GeometryObject>(parent_ptr);

        return to_c(&geo);
    }

    void retro_geometry_set_render_data(const Retro_NodeHandle node,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto *base = from_c(node);
        if (base == nullptr)
        {
            return;
        }

        auto *geo = dynamic_cast<retro::GeometryObject *>(base);
        if (geo == nullptr)
        {
            return;
        }

        geo->geometry() = retro::Geometry{
            .vertices = std::span{from_c(vertices), static_cast<usize>(vertex_count)} | std::ranges::to<std::vector>(),
            .indices = std::span{indices, static_cast<usize>(index_count)} | std::ranges::to<std::vector>()};
    }
}
