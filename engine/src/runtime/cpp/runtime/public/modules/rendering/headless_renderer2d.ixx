/**
 * @file headless_renderer2d.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.headless_renderer2d;

import std;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.runtime.rendering.render_pipeline;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.render_target;

namespace retro
{
    export class HeadlessRenderer2D final : public Renderer2D
    {
      public:
        explicit inline HeadlessRenderer2D(std::shared_ptr<RenderTarget> target) : target_{std::move(target)}
        {
        }

        inline void request_stop() override
        {
            // No-op for headless renderer
        }

        inline void wait_for_current_frame() override
        {
            // No-op for headless renderer
        }

        inline void queue_frame_for_render(RenderQueueFn factory) override
        {
            // No-op for headless renderer
        }

        inline void render_next_available_frame() override
        {
            // No-op for headless renderer
        }

        inline void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) override
        {
            // No-op for headless renderer
        }

        inline void remove_render_pipeline(std::type_index type) override
        {
            // No-op for headless renderer
        }

        [[nodiscard]] inline std::shared_ptr<RenderTarget> render_target() const override
        {
            return target_;
        }

      private:
        std::shared_ptr<RenderTarget> target_;
    };
} // namespace retro
