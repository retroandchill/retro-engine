/**
 * @file renderer_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

module retro.runtime.rendering.pipeline_manager.renderer_manager;

import retro.logging;
#include "retro/core/macros.hpp"

namespace retro
{

    RendererManager::RendererManager(PlatformBackend &platform_backend_,
                                     RenderBackend &render_backend,
                                     ViewportManager &viewports,
                                     PipelineManager pipeline_manager)
        : platform_backend_{platform_backend_}, render_backend_{render_backend}, viewports_{viewports},
          pipeline_manager_{std::move(pipeline_manager)}
    {
        viewports_.on_viewport_created().add(
            [this](Viewport &viewport)
            {
                if (const auto pr = primary_renderer(); pr.has_value())
                {
                    viewport.set_window(pr->window());
                }
            });
    }

    PlatformResult<RefCountPtr<Window>> RendererManager::create_new_window(WindowDesc window_desc)
    {
        EXPECT_ASSIGN(const auto window, platform_backend_.create_window(std::move(window_desc)));
        add_window(*window);
        return std::move(window);
    }

    Task<PlatformResult<RefCountPtr<Window>>> RendererManager::create_new_window_async(WindowDesc window_desc)
    {
        AWAIT_EXPECT_ASSIGN(const auto window, platform_backend_.create_window_async(std::move(window_desc)));
        add_window(*window);
        co_return std::move(window);
    }

    void RendererManager::add_window(Window &window)
    {
        auto create_renderer = [this, &window]
        {
            std::unique_lock lock{renderers_mutex_};
            return renderers_.emplace(window.id(), render_backend_.create_renderer(window.shared_from_this()));
        };

        auto [inserted, success] = create_renderer();
        if (!success)
        {
            get_logger().critical("Failed to add window {} to engine", window.id());
            return;
        }

        for (auto [type, pipeline] : pipeline_manager_.pipelines())
            inserted->second->add_new_render_pipeline(type, *pipeline);

        if (!primary_renderer_.has_value())
        {
            primary_renderer_ = *inserted->second;
            for (auto &viewport : viewports_.viewports())
            {
                if (!viewport->window().has_value())
                {
                    viewport->set_window(primary_renderer_->window());
                }
            }
        }
    }
    void RendererManager::remove_window(const Window &window)
    {
        const auto shared_window = window.shared_from_this();
        const auto id = window.id();

        auto get_renderer = [id, this] -> Optional<std::unique_ptr<Renderer2D>>
        {
            std::shared_lock lock{renderers_mutex_};
            const auto it = renderers_.find(id);
            if (it == renderers_.end())
                return std::nullopt;

            return std::move(it->second);
        };

        // ReSharper disable once CppTooWideScopeInitStatement
        const auto renderer = get_renderer();
        if (!renderer.has_value())
            return;
        {
            std::unique_lock lock{renderers_mutex_};
            renderers_.erase(id);
        }

        if (primary_renderer_.has_value() && primary_renderer_->window().id() == id)
        {
            if (!renderers_.empty())
            {
                primary_renderer_ = *renderers_.begin()->second;
            }
            else
            {
                primary_renderer_.reset();
            }
        }

        on_window_removed_(*shared_window);
        (*renderer)->request_stop();
    }
} // namespace retro
