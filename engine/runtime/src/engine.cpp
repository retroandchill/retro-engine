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
            .add_singleton<RenderPipeline, GeometryRenderPipeline, StoragePolicy::SharedOwned>()
            .add_singleton<RenderPipeline, SpriteRenderPipeline, StoragePolicy::SharedOwned>()
            .add_singleton<AssetSource, FileSystemAssetSource, StoragePolicy::SharedOwned>()
            .add_singleton<AssetManager>()
            .add_singleton<AssetDecoder, TextureDecoder, StoragePolicy::SharedOwned>();
    }

    Engine::Engine(ScriptRuntime &script_runtime,
                   Renderer2D &renderer,
                   PipelineManager &pipeline_manager,
                   AssetManager &asset_manager)
        : script_runtime_(script_runtime), renderer_(renderer), asset_manager_{asset_manager}, scene_{pipeline_manager}
    {
        auto &viewport = viewports_.create_viewport();
        viewport.set_scene(std::addressof(scene_));
    }

    void Engine::run(std::u16string_view assembly_path, std::u16string_view class_name, std::u16string_view entry_point)
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
        renderer_.wait_idle();
        asset_manager_.on_engine_shutdown();
    }

    void Engine::request_shutdown(const std::int32_t exit_code)
    {
        exit_code_.store(exit_code);
        running_.store(false);
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
        renderer_.begin_frame();

        const auto result = viewports_.primary().and_then(
            [](Viewport &primary)
            {
                return primary.scene().transform(
                    [&primary](Scene &scene) -> std::tuple<Viewport &, Scene &> {
                        return {primary, scene};
                    });
            });
        if (result.has_value())
        {
            auto [primary, scene] = result.value();
            const auto vp_size =
                (primary.size().x == 0 || primary.size().y == 0) ? renderer_.viewport_size() : primary.size();
            scene.collect_draw_calls(vp_size);
        }

        renderer_.end_frame();
    }
} // namespace retro
