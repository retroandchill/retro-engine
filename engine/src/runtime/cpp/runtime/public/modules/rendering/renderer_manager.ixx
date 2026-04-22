/**
 * @file renderer_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <retro/core/exports.h>

export module retro.runtime.rendering.pipeline_manager.renderer_manager;

import std;
import retro.runtime.world.viewport;
import retro.core.containers.optional;
import retro.runtime.rendering.renderer2d;
import retro.platform.backend;
import retro.platform.window;
import retro.core.async.task;
import retro.runtime.rendering.pipeline_manager;
import retro.core.functional.delegate;
import retro.runtime.rendering.render_backend;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export using OnWindowRemoved = MulticastDelegate<void(const Window &)>;

    export class RendererManager
    {
      public:
        explicit RendererManager(PlatformBackend &platform_backend_,
                                 RenderBackend &render_backend,
                                 ViewportManager &viewports,
                                 PipelineManager pipeline_manager);

        [[nodiscard]] inline Optional<Renderer2D &> primary_renderer() const noexcept
        {
            return primary_renderer_;
        }

        RETRO_API PlatformResult<RefCountPtr<Window>> create_new_window(WindowDesc window_desc);

        RETRO_API Task<PlatformResult<RefCountPtr<Window>>> create_new_window_async(WindowDesc window_desc);

        RETRO_API void add_window(Window &window);

        RETRO_API void remove_window(const Window &window);

        inline OnWindowRemoved::Event on_window_removed()
        {
            return on_window_removed_;
        }

      private:
        PlatformBackend &platform_backend_;
        RenderBackend &render_backend_;
        ViewportManager &viewports_;
        PipelineManager pipeline_manager_;
        mutable std::shared_mutex renderers_mutex_;
        std::map<std::uint64_t, std::unique_ptr<Renderer2D>> renderers_;
        Optional<Renderer2D &> primary_renderer_;

        OnWindowRemoved on_window_removed_;
    };
} // namespace retro
