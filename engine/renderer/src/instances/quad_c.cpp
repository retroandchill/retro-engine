/**
 * @file quad_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

#include "retro/renderer/instances/quad.h"

import retro.core;
import retro.runtime;
import retro.renderer;

namespace
{
    constexpr retro::DefaultHandle from_c(const Retro_DefaultHandle id)
    {
        return retro::ViewportID{id.index, id.generation};
    }

    constexpr retro::Color from_c(const Retro_Color color)
    {
        return retro::Color{color.red, color.green, color.blue, color.alpha};
    }

    constexpr retro::Vector2f from_c(const Retro_Vector2f vector)
    {
        return retro::Vector2f{vector.x, vector.y};
    }
} // namespace

extern "C"
{
    void retro_quad_set_size(const Retro_RenderObjectId id, const Retro_Vector2f size)
    {
        retro::Engine::instance()
            .scene()
            .get_render_object(from_c(id))
            .map([](auto &c) -> auto & { return static_cast<retro::QuadRenderComponent &>(c); })
            .value()
            .set_size(from_c(size));
    }

    void retro_quad_set_color(const Retro_RenderObjectId id, const Retro_Color color)
    {
        retro::Engine::instance()
            .scene()
            .get_render_object(from_c(id))
            .map([](auto &c) -> auto & { return static_cast<retro::QuadRenderComponent &>(c); })
            .value()
            .set_color(from_c(color));
    }
}
