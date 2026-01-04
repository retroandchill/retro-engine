/**
 * @file render_component.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime;

namespace retro
{
    void RenderComponent::on_attach()
    {
        assert(!render_proxy_id_.has_value());
        create_render_proxy(Engine::instance().scene().render_proxy_manager());
    }

    void RenderComponent::on_detach()
    {
        assert(render_proxy_id_.has_value());
        destroy_render_proxy(Engine::instance().scene().render_proxy_manager(), *render_proxy_id_);
    }
} // namespace retro
