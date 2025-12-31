//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_component;

import :scene.component;
import :scene.rendering.render_proxy_manager;

namespace retro
{
    export class RETRO_API RenderComponent : public Component
    {
      public:
        void on_attach() override;

        void on_detach() override;

        virtual void create_render_proxy(RenderProxyManager &proxy_manager) = 0;

        virtual void destroy_render_proxy(RenderProxyManager &proxy_manager) = 0;
    };
} // namespace retro