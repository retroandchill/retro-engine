/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import retro.logging;

using namespace retro;

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

std::unique_ptr<Engine> Engine::instance_{};

Engine::Engine(const EngineConfig &config)
    : script_runtime_(config.script_runtime_factory()), renderer_(config.renderer_factory())
{
}

void Engine::run(std::u16string_view assembly_path, std::u16string_view class_name, std::u16string_view entry_point)
{
    using clock = std::chrono::steady_clock;
    constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

    running_.store(true);
    scene_ = std::make_unique<Scene>(renderer_.get());

    if (script_runtime_->start_scripts(assembly_path, class_name) != 0)
        return;

    // FPS tracking state
    float fps_timer = 0.0f;
    uint64 fps_frames = 0;

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

    script_runtime_->tear_down();
}

void Engine::request_shutdown(const int32 exit_code)
{
    exit_code_.store(exit_code);
    running_.store(false);
}

void Engine::tick(const float delta_time) const
{
    script_runtime_->tick(delta_time);
    scene_->update_transforms();
}

// ReSharper disable once CppMemberFunctionMayBeConst
void Engine::render()
{
    renderer_->begin_frame();

    scene_->collect_draw_calls(renderer_->viewport_size());

    renderer_->end_frame();
}
