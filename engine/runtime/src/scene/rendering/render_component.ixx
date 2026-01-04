/**
 * @file render_component.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_component;

import :scene.component;
import :scene.rendering.render_proxy_manager;

namespace retro
{
    export class RETRO_API RenderComponent : public Component
    {
      protected:
        inline explicit RenderComponent(const ComponentID &id, Entity &owner) : Component{id, owner}
        {
        }

      public:
        void on_attach() override;

        void on_detach() override;

      protected:
        virtual RenderProxyID create_render_proxy(RenderProxyManager &proxy_manager) = 0;

        virtual void destroy_render_proxy(RenderProxyManager &proxy_manager, RenderProxyID id) = 0;

      private:
        std::optional<RenderProxyID> render_proxy_id_;
    };
} // namespace retro
