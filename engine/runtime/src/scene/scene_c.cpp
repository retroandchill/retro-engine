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

    constexpr const retro::Transform &from_c(const Retro_Transform *transform)
    {
        static_assert(sizeof(Retro_Transform) == sizeof(retro::Transform));
        static_assert(alignof(Retro_Transform) == alignof(retro::Transform));
        return *std::bit_cast<const retro::Transform *>(transform);
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

    Retro_RenderObjectId retro_render_object_create(const Retro_Name name,
                                                    const Retro_ViewportId viewport_id,
                                                    const void *payload,
                                                    const int32 size)
    {
        const auto &component = retro::RenderObjectRegistry::instance().create(
            from_c(name),
            from_c(viewport_id),
            std::span{static_cast<const std::byte *>(payload), static_cast<usize>(size)});
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
}
