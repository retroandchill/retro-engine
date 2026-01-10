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

    RETRO_API retro::ViewportID retro_viewport_create()
    {
        const auto &viewport = retro::Engine::instance().scene().create_viewport();
        return viewport.id();
    }

    RETRO_API void retro_viewport_dispose(const retro::ViewportID id)
    {
        retro::Engine::instance().scene().destroy_viewport(id);
    }

    RETRO_API retro::RenderObjectID retro_render_object_create(const retro::Name name,
                                                               const retro::ViewportID viewport_id,
                                                               const std::byte *payload,
                                                               const int32 size)
    {
        const auto &component =
            retro::RenderObjectRegistry::instance().create(name,
                                                           viewport_id,
                                                           std::span{payload, static_cast<usize>(size)});
        return component.id();
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
