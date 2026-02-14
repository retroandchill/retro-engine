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
        services.add_singleton<Engine>()
            .add_singleton<PipelineManager>()
            .add_singleton<RenderPipeline, GeometryRenderPipeline>()
            .add_singleton<RenderPipeline, SpriteRenderPipeline>()
            .add_singleton<AssetSource, FileSystemAssetSource>()
            .add_singleton<AssetManager>()
            .add_singleton<AssetDecoder, TextureDecoder>();
    }

    Engine::Engine(ServiceScopeFactory &service_scope_factory,
                   ScriptRuntime &script_runtime,
                   PipelineManager &pipeline_manager,
                   AssetManager &asset_manager)
        : service_scope_factory_{service_scope_factory}, script_runtime_(script_runtime), asset_manager_{asset_manager},
          pipeline_manager_{pipeline_manager}
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
                            auto renderer = renderers_.find(old_window_ptr->id());
                            if (renderer != renderers_.end())
                            {
                                renderer->second->remove_viewport(vp);
                            }
                        }

                        if (new_window_ptr != nullptr)
                        {
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

                const auto renderer = renderers_.find(*window_id);
                if (renderer == renderers_.end())
                    return;

                renderer->second->add_viewport(viewport);
            });
        viewports_.on_viewport_destroyed().add(
            [this](Viewport &viewport)
            {
                const auto window_id =
                    viewport.window().transform([](const std::shared_ptr<Window> &window) { return window->id(); });
                if (!window_id.has_value())
                    return;

                const auto renderer = renderers_.find(*window_id);
                if (renderer == renderers_.end())
                    return;

                renderer->second->remove_viewport(viewport);
            });
    }

    void Engine::run(const std::u16string_view assembly_path, const std::u16string_view class_name)
    {
        TaskScheduler::Scope task_scope{&scheduler_};
        using clock = std::chrono::steady_clock;
        constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

        running_.store(true);

        if (script_runtime_.start_scripts(assembly_path, class_name) != 0)
            return;

        // FPS tracking state
        float fps_timer = 0.0f;
        std::uint64_t fps_frames = 0;

        auto last_frame_start = clock::now();

        while (true)
        {
            const auto frame_start = clock::now();
            const std::chrono::duration<float> frame_delta = frame_start - last_frame_start;
            const float delta_time = frame_delta.count();
            last_frame_start = frame_start;

            if (!running_.load())
            {
                break;
            }

            tick(delta_time);
            render();

            // Measure how long the work actually took
            const auto frame_end = clock::now();
            const std::chrono::duration<float> work_time = frame_end - frame_start;

            // Sleep to hit target frame duration (if work was faster than target)
            if (const float work_seconds = work_time.count(); work_seconds < target_frame_time)
            {
                const float remaining = target_frame_time - work_seconds;
                precise_wait(
                    std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::duration<float>(remaining)));
            }

            // FPS accumulation (based on actual frame length)
            const auto frame_finish = clock::now();
            const std::chrono::duration<float> full_frame = frame_finish - frame_start;
            const float full_frame_seconds = full_frame.count();

            fps_timer += full_frame_seconds;
            ++fps_frames;

            if (fps_timer >= 1.0f)
            {
                const float fps = static_cast<float>(fps_frames) / fps_timer;
                get_logger().info("FPS: {:.2f}", fps);

                fps_timer = 0.0f;
                fps_frames = 0;
            }
        }

        script_runtime_.tear_down();
        for (const auto &renderer : renderers_ | std::views::values)
        {
            renderer->wait_idle();
        }
        asset_manager_.on_engine_shutdown();
    }

    void Engine::request_shutdown(const std::int32_t exit_code)
    {
        exit_code_.store(exit_code);
        running_.store(false);
    }

    void Engine::add_window(Window &window)
    {
        auto [inserted, success] =
            renderers_.emplace(window.id(), RendererRef{window.shared_from_this(), service_scope_factory_});
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
        }
    }

    void Engine::remove_window(const Window &window)
    {
        const auto id = window.id();
        renderers_.erase(id);

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
    }

    bool Engine::remove_asset_from_cache(const AssetPath &path) const
    {
        return asset_manager_.remove_asset_from_cache(path);
    }

    void Engine::tick(const float delta_time)
    {
        scheduler_.pump();
        script_runtime_.tick(delta_time);
    }

    // ReSharper disable once CppMemberFunctionMayBeConst
    void Engine::render()
    {
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
