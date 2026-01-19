/**
 * @file scene_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/runtime/scene/scene.h"

import retro.core;
import retro.runtime;
import std;
import entt;

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

    constexpr retro::Name from_c(const Retro_Name name)
    {
        static_assert(sizeof(retro::Name) == sizeof(Retro_Name) && alignof(retro::Name) == alignof(Retro_Name));
        static_assert(sizeof(retro::NameEntryId) == sizeof(Retro_NameId) &&
                      alignof(retro::NameEntryId) == alignof(Retro_NameId));
        return std::bit_cast<retro::Name>(name);
    }

    const retro::Vector2f from_c(const Retro_Vector2f vector)
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
    Retro_EntityId retro_viewport_create(const Retro_Vector2f viewport_size)
    {
        return to_c(retro::Engine::instance().scene().create_viewport(from_c(viewport_size)));
    }

    void retro_entity_dispose(const Retro_EntityId viewport_id)
    {
        retro::Engine::instance().scene().destroy_entity(from_c(viewport_id));
    }

    Retro_EntityId retro_entity_create(const Retro_Name name, const Retro_EntityId viewport_id)
    {
        return to_c(retro::RenderObjectRegistry::instance().create(from_c(name), from_c(viewport_id)));
    }

    void retro_render_object_set_transform(const Retro_EntityId render_object_id, const Retro_Transform *transform)
    {
        auto &trans = retro::Engine::instance().scene().get_component<retro::Transform>(from_c(render_object_id));

        trans.position = from_c(transform->position);
        trans.rotation = transform->rotation;
        trans.scale = from_c(transform->scale);
        trans.dirty = true;
    }

    void retro_geometry_set_render_data(const Retro_EntityId render_object_id,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto &geo_object =
            retro::Engine::instance().scene().get_component<retro::GeometryRenderObject>(from_c(render_object_id));

        geo_object.geometry = retro::Geometry{
            .vertices = std::span{from_c(vertices), static_cast<usize>(vertex_count)} | std::ranges::to<std::vector>(),
            .indices = std::span{indices, static_cast<usize>(index_count)} | std::ranges::to<std::vector>()};
    }
}
