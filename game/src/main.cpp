//
// Created by fcors on 12/19/2025.
//
#define SDL_MAIN_HANDLED
#include <SDL3/SDL_main.h>

import retro.runtime.engine;
import std;

using namespace retro::runtime;

int main() {

    SDL_SetMainReady();

    try {
        EngineLifecycle engine_lifecycle{"Retro Engine - SDL3 RAII Window", 1280, 720};

        auto &engine = Engine::instance();
        engine.run();
        return 0;
    } catch (const std::exception& ex) {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }

}