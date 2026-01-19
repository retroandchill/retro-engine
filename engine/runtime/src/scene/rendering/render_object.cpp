/**
 * @file render_object.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime;

namespace retro
{
    /*
    Viewport &RenderObject::viewport() const noexcept
    {
        return viewport_.get();
    }
    */

    void RenderObject::on_attach()
    {
        assert(!render_proxy_id_.has_value());
        create_render_proxy(Engine::instance().scene().render_proxy_manager());
    }

    void RenderObject::on_detach()
    {
        assert(render_proxy_id_.has_value());
        destroy_render_proxy(Engine::instance().scene().render_proxy_manager(), *render_proxy_id_);
    }
} // namespace retro
