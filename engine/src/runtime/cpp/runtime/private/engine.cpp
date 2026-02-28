/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
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
    static void precise_wait(const std::chrono::microseconds duration)
    {
        const auto start = std::chrono::steady_clock::now();
        const auto end = start + duration;

        if (duration > std::chrono::milliseconds(5))
        {
            std::this_thread::sleep_for(duration - std::chrono::milliseconds(5));
        }

        // Busy-wait for the remaining short duration to maximize precision
        while (std::chrono::steady_clock::now() < end)
        {
            // Spin lock: do nothing, consume CPU cycles
        }
    }

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

    void Engine::run(const EngineCallbacks &callbacks)
    {
        TaskScheduler::Scope task_scope{&scheduler_};
        using Clock = std::chrono::steady_clock;
        constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

        running_.store(true);

        if (callbacks.start() != 0)
            return;

        // FPS tracking state
        float fps_timer = 0.0f;
        std::uint64_t fps_frames = 0;

        auto last_frame_start = Clock::now();

        while (true)
        {
            const auto frame_start = Clock::now();
            const std::chrono::duration<float> frame_delta = frame_start - last_frame_start;
            const float delta_time = frame_delta.count();
            last_frame_start = frame_start;

            if (!running_.load())
            {
                break;
            }

            scheduler_.pump();
            callbacks.tick(delta_time);
            render();

            // Measure how long the work actually took
            const auto frame_end = Clock::now();
            const std::chrono::duration<float> work_time = frame_end - frame_start;

            // Sleep to hit target frame duration (if work was faster than target)
            if (const float work_seconds = work_time.count(); work_seconds < target_frame_time)
            {
                const float remaining = target_frame_time - work_seconds;
                precise_wait(
                    std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::duration<float>(remaining)));
            }

            // FPS accumulation (based on actual frame length)
            const auto frame_finish = Clock::now();
            const std::chrono::duration<float> full_frame = frame_finish - frame_start;
            const float full_frame_seconds = full_frame.count();

            fps_timer += full_frame_seconds;
            ++fps_frames;

            if (fps_timer >= 1.0f)
            {
                fps_timer = 0.0f;
                fps_frames = 0;
            }
        }

        callbacks.stop();
        {
            std::unique_lock lock{renderers_mutex_};
            for (const auto &renderer : renderers_ | std::views::values)
            {
                renderer->wait_idle();
            }
        }
        asset_manager_.on_engine_shutdown();
    }

    std::int32_t Engine::run_platform_event_loop()
    {
        while (running_.load())
        {
            while (auto event = platform_backend_.wait_for_event(std::chrono::milliseconds(10)))
            {
                std::visit(
                    [&]<typename T>(const T &evt)
                    {
                        if constexpr (std::is_same_v<T, QuitEvent>)
                        {
                            if (running_.load())
                            {
                                request_shutdown();
                            }
                        }
                        else if constexpr (std::is_same_v<T, WindowCloseRequestedEvent>)
                        {
                            Window *window = nullptr;
                            {
                                std::shared_lock lock{renderers_mutex_};
                                auto renderer = renderers_.find(evt.window_id);
                                if (renderer == renderers_.end())
                                    return;
                                window = std::addressof(renderer->second->window());
                            }

                            remove_window(*window);
                        }
                    },
                    *event);

                if (!running_.load())
                {
                    break;
                }
            }
        }

        return exit_code_.load();
    }

    void Engine::request_shutdown(const std::int32_t exit_code)
    {
        exit_code_.store(exit_code);
        running_.store(false);
    }

    Window &Engine::create_new_window(const WindowDesc &window_desc)
    {
        const auto window = platform_backend_.create_window(window_desc);
        add_window(*window);
        return *window;
    }
    Optional<Window &> Engine::get_window(const std::uint64_t window_id)
    {
        std::shared_lock lock{renderers_mutex_};
        if (const auto it = renderers_.find(window_id); it != renderers_.end())
        {
            return it->second->window();
        }

        return std::nullopt;
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

        // We need to ensure the renderer is not actively in the middle of a render pass before
        // Letting it go out of scope and get destroyed
        renderer->get().wait_idle();
    }

    bool Engine::remove_asset_from_cache(const AssetPath &path) const
    {
        return asset_manager_.remove_asset_from_cache(path);
    }

    // ReSharper disable once CppMemberFunctionMayBeConst
    void Engine::render()
    {
        std::shared_lock lock{renderers_mutex_};
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
    }
} // namespace retro
