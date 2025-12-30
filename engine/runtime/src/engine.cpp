//
// Created by fcors on 12/23/2025.
//
module retro.runtime;

using namespace retro;

std::unique_ptr<Engine> Engine::instance_{};

Engine::Engine(const EngineConfig &config)
    : script_runtime(config.script_runtime_factory()), renderer_(config.renderer_factory())
{
}

void Engine::run()
{
    using clock = std::chrono::steady_clock;
    constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

    running_.store(true);

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
        const float work_seconds = work_time.count();

        // Sleep to hit target frame duration (if work was faster than target)
        if (work_seconds < target_frame_time)
        {
            const float remaining = target_frame_time - work_seconds;
            std::this_thread::sleep_for(
                std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::duration<float>(remaining)));
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
            std::println("FPS: {:.2f}", fps);

            fps_timer = 0.0f;
            fps_frames = 0;
        }
    }
}

void Engine::request_shutdown()
{
    running_.store(false);
}

void Engine::tick(float delta_time)
{
    // TODO: Add tick logic
}

// ReSharper disable once CppMemberFunctionMayBeConst
void Engine::render()
{
    renderer_->begin_frame();

    constexpr int width = 1280 / 100 + 1;
    constexpr int height = 720 / 100 + 1;
    for (int i = 0; i < width; i++)
    {
        for (int j = 0; j < height; j++)
        {
            const int index = i + j * width;           // linear index if needed
            const float r = (index & 1) ? 1.0f : 0.0f; // bit 0
            const float g = (index & 2) ? 1.0f : 0.0f; // bit 1
            const float b = (index & 4) ? 1.0f : 0.0f; // bit 2

            const Color c{r, g, b, 1.0f};
            renderer_->draw_quad({i * 100.0f, j * 100.0f}, {100.0f, 100.0f}, c);
        }
    }

    renderer_->end_frame();
}
