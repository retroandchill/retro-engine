/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.runtime.engine;

import retro.logging;
import retro.core.async.task_scheduler;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.filesystem_asset_source;
import retro.runtime.assets.textures.texture_decoder;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.core.containers.optional;
import retro.core.math.vector;
import retro.platform.event;

namespace retro
{

    Engine *Engine::instance_{};

    void add_engine_services(ServiceCollection &services)
    {
        services.add_singleton<PipelineManager>()
            .add_singleton<RenderPipeline, GeometryRenderPipeline>()
            .add_singleton<RenderPipeline, SpriteRenderPipeline>()
            .add_singleton<AssetSource, FileSystemAssetSource>()
            .add_singleton<AssetManager>()
            .add_singleton<AssetDecoder, TextureDecoder>();
    }

    Engine::Engine(std::shared_ptr<ServiceProvider> service_provider)
        : service_provider_{std::move(service_provider)},
          platform_backend_{service_provider_->get_required<PlatformBackend>()},
          service_scope_factory_{service_provider_->get_required<ServiceScopeFactory>()},
          asset_manager_{service_provider_->get_required<AssetManager>()},
          pipeline_manager_{service_provider_->get_required<PipelineManager>()}
    {
        viewports_.on_viewport_created().add(
            [this](Viewport &viewport)
            {
                viewport.on_window_changed().add(
                    [this](Viewport &vp, const std::weak_ptr<Window> &old_win, const std::weak_ptr<Window> &new_win)
                    {
                        const auto old_window_ptr = old_win.lock();
                        const auto new_window_ptr = new_win.lock();

                        if (old_window_ptr == new_window_ptr)
                            return;

                        if (old_window_ptr != nullptr)
                        {
                            std::shared_lock lock{renderers_mutex_};
                            if (const auto renderer = renderers_.find(old_window_ptr->id());
                                renderer != renderers_.end())
                            {
                                renderer->second->remove_viewport(vp);
                            }
                        }

                        if (new_window_ptr != nullptr)
                        {
                            std::shared_lock lock{renderers_mutex_};
                            auto renderer = renderers_.find(new_window_ptr->id());
                            if (renderer != renderers_.end())
                            {
                                renderer->second->add_viewport(vp);
                            }
                        }
                    });

                const auto window_id =
                    viewport.window().transform([](const std::shared_ptr<Window> &window) { return window->id(); });
                if (!window_id.has_value())
                    return;

                auto get_renderer = [window_id, this] -> Optional<RendererRef>
                {
                    std::shared_lock lock{renderers_mutex_};
                    const auto it = renderers_.find(*window_id);
                    if (it == renderers_.end())
                        return std::nullopt;

                    return it->second;
                };

                const auto renderer = get_renderer();
                if (!renderer.has_value())
                    return;

                (*renderer)->add_viewport(viewport);
            });
        viewports_.on_viewport_destroyed().add(
            [this](Viewport &viewport)
            {
                const auto window_id =
                    viewport.window().transform([](const std::shared_ptr<Window> &window) { return window->id(); });
                if (!window_id.has_value())
                    return;

                std::shared_lock lock{renderers_mutex_};
                const auto renderer = renderers_.find(*window_id);
                if (renderer == renderers_.end())
                    return;

                renderer->second->remove_viewport(viewport);
            });
    }

    void Engine::pump_tasks(const std::size_t max)
    {
        scheduler_.pump(max);
    }

    // ReSharper disable once CppMemberFunctionMayBeConst
    void Engine::render()
    {
        std::shared_lock lock{renderers_mutex_};
        if (renderers_.empty())
        {
            // If there are no active renderers to draw to then we run the risk of
            // the game loop running continuously and using too much CPU, so in that case
            // we're going to request the thread sleep for a bit
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
            return;
        }

        for (const auto &renderer : renderers_ | std::views::values)
        {
            renderer->begin_frame();

            for (auto &viewport : viewports_.viewports())
            {
                auto scene = viewport->scene();
                if (!scene.has_value())
                    continue;

                pipeline_manager_.collect_all_draw_calls(scene->nodes(), renderer->window().size(), *viewport);
            }

            renderer->end_frame();
        }

        // We want to wait for the fences on all the renderers after the loop is done
        // If we waited on each one before all frames gets submitted, it would end up
        // blocking the game thread for too long if there are many surfaces.
        for (const auto &renderer : renderers_ | std::views::values)
        {
            renderer->wait_for_current_frame();
        }
    }

    void Engine::on_loop_exit()
    {
        renderers_.clear();
        asset_manager_.on_engine_shutdown();
    }

    void Engine::wait_platform_event(const std::chrono::milliseconds timeout)
    {
        if (auto event = platform_backend_.wait_for_event(timeout))
        {
            handle_platform_event(*event);
        }
    }

    void Engine::poll_events_once()
    {
        while (auto event = platform_backend_.poll_event())
        {
            if (!handle_platform_event(*event))
            {
                // ReSharper disable once CppDFAUnreachableCode
                break;
            }
        }
    }

    Task<PlatformResult<std::shared_ptr<Window>>> Engine::create_new_window(WindowDesc window_desc)
    {
        AWAIT_EXPECT_ASSIGN(const auto window, platform_backend_.create_window_async(std::move(window_desc)));
        add_window(*window);
        co_return std::move(window);
    }

    void Engine::add_window(Window &window)
    {
        auto create_renderer = [this, &window]
        {
            std::unique_lock lock{renderers_mutex_};
            return renderers_.emplace(window.id(), RendererRef{window.shared_from_this(), service_scope_factory_});
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

    void Engine::remove_window(const Window &window)
    {
        const auto shared_window = window.shared_from_this();
        const auto id = window.id();

        auto get_renderer = [id, this] -> Optional<RendererRef>
        {
            std::shared_lock lock{renderers_mutex_};
            const auto it = renderers_.find(id);
            if (it == renderers_.end())
                return std::nullopt;

            return it->second;
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
    }

    bool Engine::remove_asset_from_cache(const AssetPath &path) const
    {
        return asset_manager_.remove_asset_from_cache(path);
    }

    bool Engine::handle_platform_event(const Event &event)
    {
        return std::visit(
            [&]<typename T>(const T &evt)
            {
                if constexpr (std::is_same_v<T, QuitEvent>)
                {
                    on_shutdown_requested_();
                    return false;
                }
                else if constexpr (std::is_same_v<T, WindowCloseRequestedEvent>)
                {
                    Window *window = nullptr;
                    {
                        std::shared_lock lock{renderers_mutex_};
                        auto renderer = renderers_.find(evt.window_id);
                        if (renderer == renderers_.end())
                            return true;
                        window = std::addressof(renderer->second->window());
                    }

                    remove_window(*window);
                }
                else if constexpr (std::is_same_v<T, CallbackEvent>)
                {
                    if (evt.callback)
                    {
                        evt.callback();
                    }
                }

                return true;
            },
            event);
    }
} // namespace retro
