//
// Created by fcors on 12/23/2025.
//
module;

#include <SDL3/SDL.h>

module retro.runtime;

using namespace retro;

std::unique_ptr<Engine> Engine::instance_{};

void Engine::run()
{
    using clock = std::chrono::steady_clock;
    constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

    auto last_time = clock::now();

    while (true)
    {
        auto now = clock::now();
        std::chrono::duration<float> frame_delta = now - last_time;
        last_time = now;

        const float delta_time = frame_delta.count();

        if (poll_events())
        {
            break;
        }

        tick(delta_time);
        render();

        if (delta_time < target_frame_time)
        {
            auto sleep_time = std::chrono::duration<float>(target_frame_time - delta_time);

            std::this_thread::sleep_for(std::chrono::duration_cast<std::chrono::milliseconds>(sleep_time));
        }
    }
}

bool Engine::poll_events()
{
    SDL_Event event;
    bool should_quit = false;

    while (SDL_PollEvent(&event))
    {
        switch (event.type)
        {
            case SDL_EVENT_QUIT:
            case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                // For now, we only have one window, so any close means quit.
                should_quit = true;
                break;
            default:
                break;
        }
    }

    return should_quit;
}

void Engine::tick(float delta_time)
{
    // TODO: Add tick logic
}

// ReSharper disable once CppMemberFunctionMayBeConst
void Engine::render()
{
    renderer_->begin_frame();

    renderer_->draw_quad({0.0f, 0.0f}, {100.0f, 100.0f}, {1.0f, 0.0f, 0.0f, 1.0f});

    renderer_->end_frame();
}
