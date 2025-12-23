//
// Created by fcors on 12/19/2025.
//
#define SDL_MAIN_HANDLED
#include <SDL3/SDL_main.h>

import retro.runtime.engine;
import retro.scripting.dotnet;
import std;

using namespace retro::runtime;
using namespace retro::scripting;

int main() {

    SDL_SetMainReady();

    try {
        EngineLifecycle engine_lifecycle{"Retro Engine - SDL3 RAII Window", 1280, 720};

        DotnetLifecycle dotnet_lifecycle;

        auto &engine = Engine::instance();
        engine.run();
        return 0;
    } catch (const std::exception& ex) {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }

}