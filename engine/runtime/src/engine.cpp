//
// Created by fcors on 12/23/2025.
//
module retro.runtime.engine;

using namespace retro::runtime;

std::unique_ptr<Engine> Engine::instance_{};

void Engine::run() {
    using clock = std::chrono::steady_clock;
    constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

    auto last_time = clock::now();

    while (true) {
        auto now = clock::now();
        std::chrono::duration<float> frame_delta = now - last_time;
        last_time = now;

        const float delta_time = frame_delta.count();

        if (platform_.poll_events()) {
            break;
        }

        tick(delta_time);
        render();

        if (delta_time < target_frame_time) {
            auto sleep_time = std::chrono::duration<float>(target_frame_time - delta_time);

            std::this_thread::sleep_for(
                std::chrono::duration_cast<std::chrono::milliseconds>(sleep_time)
            );
        }
    }
}

void Engine::tick(float delta_time) {
    // TODO: Add tick logic
}

void Engine::render() {
    // TODO: Add render logic
}
