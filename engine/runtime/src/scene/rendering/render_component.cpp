//
// Created by fcors on 12/31/2025.
//

module retro.runtime;

namespace retro
{
    void RenderComponent::on_attach()
    {
        create_render_proxy(Engine::instance().scene().render_proxy_manager());
    }

    void RenderComponent::on_detach()
    {
        destroy_render_proxy(Engine::instance().scene().render_proxy_manager());
    }
} // namespace retro
