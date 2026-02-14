/**
 * @file renderer2d.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.renderer2d;

namespace retro
{
    namespace
    {
        std::shared_ptr<ServiceScope> create_service_scope(ServiceScopeFactory &factory, std::shared_ptr<Window> window)
        {
            auto scope = factory.create_scope([window](ServiceCollection &collection) { collection.add(window); });
            return scope;
        }
    } // namespace

    RendererRef::RendererRef(std::shared_ptr<Window> window, ServiceScopeFactory &scope_factory)
        : scope_(create_service_scope(scope_factory, std::move(window))),
          renderer_{scope_->service_provider().get_required<Renderer2D>()}
    {
    }
} // namespace retro
