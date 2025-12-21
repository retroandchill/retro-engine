//
// Created by fcors on 12/19/2025.
//
#define SDL_MAIN_HANDLED
#include <SDL3/SDL_main.h>

import retro.runtime.engine;
import std;

int main() {
    using namespace retro::runtime;

    SDL_SetMainReady();

    try {
        Engine engine{"Retro Engine - SDL3 RAII Window", 1280, 720};
        engine.run();
        return 0;
    } catch (const std::exception& ex) {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }

}