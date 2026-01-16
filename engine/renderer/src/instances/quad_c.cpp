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
    void retro_quad_update_data(const Retro_RenderObjectId id, const Retro_QuadUpdateData *color)
    {
        auto &render_object = retro::Engine::instance().scene().get_render_object(from_c(id)).value();
        auto &quad = static_cast<retro::QuadRenderComponent &>(render_object);
        quad.set_size(from_c(color->size));
        quad.set_color(from_c(color->color));
    }
}
