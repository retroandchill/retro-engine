//
// Created by fcors on 12/19/2025.
//
#define SDL_MAIN_HANDLED
#include <SDL3/SDL_main.h>

import retro.runtime;
import retro.scripting;
import retro.renderer;
import std;

using namespace retro;

int main()
{
    SDL_SetMainReady();
    SdlRuntime sdl_runtime;

    try
    {
        const EngineConfig config{.script_runtime_factory = [&] { return std::make_unique<DotnetManager>(); },
                                  .renderer_factory =
                                      []
                                  {
                                      return std::make_unique<VulkanRenderer2D>(Window{1280, 720, "Retro Engine"});
                                  }};
        EngineLifecycle engine_lifecycle{config};

        auto &engine = Engine::instance();
        engine.run();
        return 0;
    }
    catch (const std::exception &ex)
    {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }
}