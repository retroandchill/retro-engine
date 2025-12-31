//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_component;

import :scene.component;

namespace retro
{
    export class RETRO_API RenderComponent : public Component
    {
      public:
        void on_attach() override;

        void on_detach() override;
    };
} // namespace retro