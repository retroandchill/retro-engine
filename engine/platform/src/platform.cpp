//
// Created by fcors on 12/23/2025.
//
module;
#include <SDL3/SDL.h>

module retro.platform;

using namespace retro;

// NOLINTNEXTLINE
bool Platform::poll_events() {
    SDL_Event event;
    bool should_quit = false;

    while (SDL_PollEvent(&event)) {
        switch (event.type) {
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
