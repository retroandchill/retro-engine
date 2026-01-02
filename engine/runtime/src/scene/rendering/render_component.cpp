//
// Created by fcors on 12/31/2025.
//
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
