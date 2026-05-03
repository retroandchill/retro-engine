/**
 * @file render_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <retro/core/exports.h>

export module retro.runtime.rendering.pipeline_manager.render_manager;

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

    export class RETRO_API RenderManager
    {
      public:
        explicit RenderManager(PlatformBackend &platform_backend,
                               RenderBackend &render_backend,
                               ViewportManager &viewports,
                               PipelineManager pipeline_manager);

        [[nodiscard]] inline Optional<Renderer2D &> primary_renderer() const noexcept
        {
            return primary_renderer_;
        }

        PlatformResult<std::uint64_t> create_new_window(WindowDesc window_desc);

        PlatformResult<std::uint64_t> create_new_window(NativeWindowHandle handle);

        Task<PlatformResult<std::uint64_t>> create_new_window_async(WindowDesc window_desc);

        Task<PlatformResult<std::uint64_t>> create_new_window_async(NativeWindowHandle handle);

        Optional<Window &> get_window(std::uint64_t window_id) const;

        void add_window(Window &window);

        void remove_window(std::uint64_t window_id);

        bool set_viewport_window(Viewport &viewport, std::uint64_t window_id) const;

        void sync_renderer_state();

        void render() const;

        void on_engine_shutdown();

        inline OnWindowRemoved::Event on_window_removed()
        {
            return on_window_removed_;
        }

      private:
        [[nodiscard]] std::vector<std::shared_ptr<Renderer2D>> get_current_renderers() const;
        [[nodiscard]] Optional<std::shared_ptr<Renderer2D>> get_renderer(std::uint64_t window_id) const;

        PlatformBackend &platform_backend_;
        RenderBackend &render_backend_;
        ViewportManager &viewports_;
        PipelineManager pipeline_manager_;
        mutable std::shared_mutex renderers_mutex_;
        std::map<std::uint64_t, std::shared_ptr<Renderer2D>> renderers_;
        Optional<Renderer2D &> primary_renderer_;

        OnWindowRemoved on_window_removed_;
    };
} // namespace retro
