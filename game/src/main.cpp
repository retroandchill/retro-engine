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
        auto window = std::make_shared<Window>(1280, 720, "Retro Engine");
        const EngineConfig config{.script_runtime_factory = [&] { return std::make_unique<DotnetManager>(); },
                                  .renderer_factory =
                                      [&]
                                  {
                                      return std::make_unique<VulkanRenderer2D>(window);
                                  }};

        std::atomic game_thread_exited = false;
        auto game_thread = std::thread{[&]
                                       {
                                           EngineLifecycle engine_lifecycle{config};
                                           auto &engine = Engine::instance();
                                           engine.run();
                                           game_thread_exited.store(true);
                                       }};

        while (!game_thread_exited.load())
        {
            SDL_Event event;
            while (SDL_WaitEventTimeout(&event, 10))
            {
                switch (event.type)
                {
                    case SDL_EVENT_QUIT:
                    case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                        if (!game_thread_exited.load())
                        {
                            Engine::instance().request_shutdown();
                        }
                        break;
                    default:
                        break;
                }

                if (game_thread_exited.load())
                {
                    break;
                }
            }
        }

        game_thread.join();

        return 0;
    }
    catch (const std::exception &ex)
    {
        std::cerr << "Fatal error: " << ex.what() << '\n';
        return -1;
    }
}