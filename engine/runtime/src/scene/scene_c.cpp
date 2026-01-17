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

namespace
{
    constexpr retro::DefaultHandle from_c(const Retro_DefaultHandle id)
    {
        return retro::ViewportID{id.index, id.generation};
    }

    constexpr Retro_DefaultHandle to_c(const retro::DefaultHandle id)
    {
        return Retro_DefaultHandle{id.index, id.generation};
    }

    constexpr retro::Name from_c(const Retro_Name name)
    {
        static_assert(sizeof(retro::Name) == sizeof(Retro_Name) && alignof(retro::Name) == alignof(Retro_Name));
        static_assert(sizeof(retro::NameEntryId) == sizeof(Retro_NameId) &&
                      alignof(retro::NameEntryId) == alignof(Retro_NameId));
        return std::bit_cast<retro::Name>(name);
    }

    const retro::Transform &from_c(const Retro_Transform *transform)
    {
        static_assert(sizeof(Retro_Transform) == sizeof(retro::Transform));
        static_assert(alignof(Retro_Transform) == alignof(retro::Transform));
        return *reinterpret_cast<const retro::Transform *>(transform);
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
    Retro_ViewportId retro_viewport_create()
    {
        const auto &viewport = retro::Engine::instance().scene().create_viewport();
        return to_c(viewport.id());
    }

    void retro_viewport_dispose(const Retro_ViewportId viewport_id)
    {
        retro::Engine::instance().scene().destroy_viewport(from_c(viewport_id));
    }

    Retro_RenderObjectId retro_render_object_create(const Retro_Name name, const Retro_ViewportId viewport_id)
    {
        const auto &component = retro::RenderObjectRegistry::instance().create(from_c(name), from_c(viewport_id));
        return to_c(component.id());
    }

    void retro_render_object_dispose(const Retro_RenderObjectId render_object_id)
    {
        retro::Engine::instance().scene().destroy_render_object(from_c(render_object_id));
    }

    void retro_render_object_set_transform(const Retro_RenderObjectId render_object_id,
                                           const Retro_Transform *transform)
    {
        retro::Engine::instance()
            .scene()
            .get_render_object(from_c(render_object_id))
            .value()
            .set_transform(from_c(transform));
    }

    void retro_geometry_set_render_data(const Retro_RenderObjectId render_object_id,
                                        const Retro_Vertex *vertices,
                                        const int32_t vertex_count,
                                        uint32_t *indices,
                                        const int32_t index_count)
    {
        auto &render_object = retro::Engine::instance().scene().get_render_object(from_c(render_object_id)).value();

        auto &geo_object = static_cast<retro::GeometryRenderObject &>(render_object);

        geo_object.set_geometry(retro::Geometry{
            .vertices = std::span{from_c(vertices), static_cast<usize>(vertex_count)} | std::ranges::to<std::vector>(),
            .indices = std::span{indices, static_cast<usize>(index_count)} | std::ranges::to<std::vector>()});
    }
}
