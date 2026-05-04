/**
 * @file render_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

module retro.runtime.rendering.pipeline_manager.render_manager;

import retro.logging;
#include "retro/core/macros.hpp"
import retro.runtime.rendering.draw_command;
import retro.runtime.world.scene;

namespace retro
{

    RenderManager::RenderManager(PlatformBackend &platform_backend,
                                 RenderBackend &render_backend,
                                 ViewportManager &viewports,
                                 PipelineManager pipeline_manager)
        : platform_backend_{platform_backend}, render_backend_{render_backend}, viewports_{viewports},
          pipeline_manager_{std::move(pipeline_manager)}
    {
        viewports_.on_viewport_created().add(
            [this](Viewport &viewport)
            {
                if (const auto pr = primary_renderer(); pr.has_value())
                {
                    viewport.set_target(pr->render_target());
                }
            });
    }

    PlatformResult<std::uint64_t> RenderManager::create_new_window(WindowDesc window_desc)
    {
        EXPECT_ASSIGN(auto window, platform_backend_.create_window(std::move(window_desc)));
        add_window(std::move(window));
        return window->id();
    }

    Task<PlatformResult<std::uint64_t>> RenderManager::create_new_window_async(WindowDesc window_desc)
    {
        AWAIT_EXPECT_ASSIGN(auto window, platform_backend_.create_window_async(std::move(window_desc)));
        add_window(std::move(window));
        co_return window->id();
    }

    void RenderManager::add_window(std::unique_ptr<Window> window)
    {
        auto renderer = render_backend_.create_renderer(std::move(window));

        for (auto [type, pipeline] : pipeline_manager_.pipelines())
            renderer->add_new_render_pipeline(type, *pipeline);

        auto target_id = renderer->render_target()->id();

        auto create_renderer = [this, target_id, &renderer]
        {
            std::unique_lock lock{renderers_mutex_};
            return renderers_.emplace(target_id, std::move(renderer));
        };

        auto [inserted, success] = create_renderer();
        if (!success)
        {
            get_logger().critical("Failed to add window {} to engine", target_id);
            return;
        }

        if (!primary_renderer_.has_value())
        {
            primary_renderer_ = *inserted->second;
            for (auto &viewport : viewports_.viewports())
            {
                if (!viewport->target().has_value())
                {
                    viewport->set_target(primary_renderer_->render_target());
                }
            }
        }
    }
    void RenderManager::remove_window(std::uint64_t window_id)
    {
        const auto renderer = get_renderer(window_id);
        if (!renderer.has_value())
            return;
        {
            std::unique_lock lock{renderers_mutex_};
            renderers_.erase(window_id);
        }

        if (primary_renderer_.has_value() && primary_renderer_->render_target()->id() == window_id)
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

        on_render_target_removed_(*(*renderer)->render_target());
        (*renderer)->request_stop();
    }

    bool RenderManager::set_viewport_window(Viewport &viewport, const std::uint64_t window_id) const
    {
        if (window_id == 0)
        {
            viewport.clear_target();
            return true;
        }

        auto renderer = get_renderer(window_id);
        if (!renderer.has_value())
            return false;

        viewport.set_target((*renderer)->render_target());
        return true;
    }

    void RenderManager::sync_renderer_state()
    {
        // ReSharper disable once CppDFAUnreadVariable
        // ReSharper disable once CppDFAUnusedValue
        constexpr auto get_scene_data =
            [](const std::unique_ptr<Viewport> &viewport) -> Optional<std::pair<Viewport &, Scene &>>
        {
            const auto scene = viewport->scene();
            if (!scene.has_value())
                return std::nullopt;

            return std::pair<Viewport &, Scene &>{*viewport, *scene};
        };

        const auto current_renderers = get_current_renderers();

        if (current_renderers.empty())
        {
            // If there are no active renderers to draw to then we run the risk of
            // the game loop running continuously and using too much CPU, so in that case
            // we're going to request the thread sleep for a bit
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
            return;
        }

        for (const auto &renderer : current_renderers)
        {
            renderer->queue_frame_for_render(
                [this, &renderer, get_scene_data](std::pmr::memory_resource &resource)
                {
                    return viewports_.viewports() | std::views::transform(get_scene_data) | std::views::join |
                           std::views::transform(
                               [this, &resource, &renderer](auto &pair)
                               {
                                   auto [viewport, scene] = pair;
                                   return pipeline_manager_.collect_draw_command_sources(
                                       scene.nodes(),
                                       renderer->render_target()->size(),
                                       viewport,
                                       resource);
                               }) |
                           std::ranges::to<std::pmr::vector<DrawCommandSet>>(&resource);
                });
        }
    }

    void RenderManager::render() const
    {
        const auto current_renderers = get_current_renderers();
        if (current_renderers.empty())
        {
            // If there are no active renderers to draw to then we run the risk of
            // the game loop running continuously and using too much CPU, so in that case
            // we're going to request the thread sleep for a bit
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
            return;
        }

        for (const auto &renderer : current_renderers)
        {
            renderer->render_next_available_frame();
        }

        // We want to wait for the fences on all the renderers after the loop is done
        // If we waited on each one before all frames gets submitted, it would end up
        // blocking the game thread for too long if there are many surfaces.
        for (const auto &renderer : current_renderers)
        {
            renderer->wait_for_current_frame();
        }
    }

    void RenderManager::on_engine_shutdown()
    {
        std::unique_lock lock{renderers_mutex_};
        {
            renderers_.clear();
        }
    }

    std::vector<std::shared_ptr<Renderer2D>> RenderManager::get_current_renderers() const
    {
        std::shared_lock lock{renderers_mutex_};
        return renderers_ | std::views::values | std::ranges::to<std::vector>();
    }

    Optional<std::shared_ptr<Renderer2D>> RenderManager::get_renderer(std::uint64_t window_id) const
    {
        std::shared_lock lock{renderers_mutex_};
        const auto it = renderers_.find(window_id);
        if (it == renderers_.end())
            return std::nullopt;

        return std::move(it->second);
    }
} // namespace retro
