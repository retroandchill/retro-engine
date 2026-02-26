/**
 * @file renderer2d.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.renderer2d;

import retro.core.containers.inline_list;
import retro.core.containers.optional;
import retro.core.math.vector;
import retro.logging;
import std;
import retro.platform.window;
import retro.core.di;
import retro.runtime.rendering.texture_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.world.viewport;

namespace retro
{
    export class Renderer2D
    {
      public:
        virtual ~Renderer2D() = default;

        virtual void wait_idle() = 0;

        virtual void begin_frame() = 0;

        virtual void end_frame() = 0;

        virtual void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) = 0;

        virtual void remove_render_pipeline(std::type_index type) = 0;

        [[nodiscard]] virtual Window &window() const = 0;

        virtual void add_viewport(Viewport &viewport) = 0;

        virtual void remove_viewport(Viewport &viewport) = 0;
    };

    export class RETRO_API RendererRef
    {
      public:
        explicit RendererRef(std::shared_ptr<Window> window, ServiceScopeFactory &scope_factory);

        [[nodiscard]] inline Renderer2D &get() const
        {
            return renderer_;
        }

        [[nodiscard]] inline Renderer2D &operator*() const
        {
            return renderer_;
        }

        [[nodiscard]] inline Renderer2D *operator->() const
        {
            return std::addressof(renderer_);
        }

      private:
        std::shared_ptr<ServiceScope> scope_;
        Renderer2D &renderer_;
    };

} // namespace retro
