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
    constexpr Retro_EntityId to_c(const entt::entity entity)
    {
        return static_cast<Retro_EntityId>(entity);
    }

    constexpr entt::entity from_c(const Retro_EntityId entity)
    {
        return static_cast<entt::entity>(entity);
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

    void retro_entity_dispose(const Retro_EntityId entity_id)
    {
        retro::Engine::instance().scene().destroy_entity(from_c(entity_id));
    }

    void retro_scene_update_transforms(const Retro_TransformUpdate *updates, int32_t update_count)
    {
        std::span update_span{updates, static_cast<usize>(update_count)};
        for (const auto &[entity_id, position, rotation, scale] : update_span)
        {
            auto &transform = retro::Engine::instance().scene().get_component<retro::Transform>(from_c(entity_id));
            transform.set_position(from_c(position));
            transform.set_rotation(rotation);
            transform.set_scale(from_c(scale));
        }
    }

    Retro_EntityId retro_viewport_create(const Retro_Vector2f viewport_size)
    {
        return to_c(retro::Engine::instance().scene().create_viewport(from_c(viewport_size)));
    }

    void retro_scene_update_viewports(const Retro_ViewUpdate *updates, const int32_t update_count)
    {
        for (auto [entity_id, viewport_size] : std::span{updates, static_cast<usize>(update_count)})
        {
            auto &[view_size] = retro::Engine::instance().scene().get_component<retro::Viewport>(from_c(entity_id));
            view_size = from_c(viewport_size);
        }
    }

    Retro_EntityId retro_geometry_create(Retro_EntityId entity_id)
    {
        auto [id, geo] = retro::Engine::instance().scene().create_render_component<retro::GeometryRenderComponent>(
            from_c(entity_id));
        return to_c(id);
    }

    void retro_geometry_set_render_data(const Retro_EntityId entity_id,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto &geo_object =
            retro::Engine::instance().scene().get_component<retro::GeometryRenderComponent>(from_c(entity_id));

        geo_object.geometry = retro::Geometry{std::span{from_c(vertices), static_cast<usize>(vertex_count)},
                                              std::span{indices, static_cast<usize>(index_count)}};
    }
}
