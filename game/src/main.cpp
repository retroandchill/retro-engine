//
// Created by fcors on 12/19/2025.
//
#define SDL_MAIN_HANDLED
#include <SDL3/SDL_main.h>

import retro.platform;
import retro.platform.window;
import std;

int main() {
    SDL_SetMainReady();

    using namespace retro::platform;

    try {
        Platform platform{};

        Window window{platform, 1280, 720, "Retro Engine - SDL3 RAII Window"};

        while (!window.should_close()) {
            window.poll_events();
        }

    } catch (const std::exception& ex) {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }

    return 0;
}