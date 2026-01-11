/**
 * @file scene_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <cassert>

import retro.core;
import retro.runtime;
import std;

extern "C"
{
    using Retro_ViewportHandle = uint64;
    using Retro_RenderObjectHandle = uint64;

    RETRO_API Retro_ViewportHandle retro_viewport_create()
    {
        static_assert(sizeof(Retro_ViewportHandle) == sizeof(retro::ViewportID));
        const auto &viewport = retro::Engine::instance().scene().create_viewport();
        return std::bit_cast<Retro_ViewportHandle>(viewport.id());
    }

    RETRO_API void retro_viewport_dispose(const retro::ViewportID id)
    {
        retro::Engine::instance().scene().destroy_viewport(id);
    }

    RETRO_API Retro_RenderObjectHandle retro_render_object_create(const retro::Name name,
                                                                  const retro::ViewportID viewport_id,
                                                                  const std::byte *payload,
                                                                  const int32 size)
    {
        static_assert(sizeof(Retro_RenderObjectHandle) == sizeof(retro::ViewportID));
        const auto &component =
            retro::RenderObjectRegistry::instance().create(name,
                                                           viewport_id,
                                                           std::span{payload, static_cast<usize>(size)});
        return std::bit_cast<Retro_RenderObjectHandle>(component.id());
    }

    RETRO_API void retro_render_object_dispose(const retro::RenderObjectID id)
    {
        retro::Engine::instance().scene().destroy_render_object(id);
    }

    RETRO_API void retro_render_object_set_transform(const retro::RenderObjectID id, const retro::Transform *transform)
    {
        retro::Engine::instance().scene().get_render_object(id).value().set_transform(*transform);
    }
}
