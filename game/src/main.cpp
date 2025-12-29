//
// Created by fcors on 12/19/20
import retro.runtime;
import retro.scripting;
import retro.renderer;
import std;

import sdl;

using namespace retro;

int main()
{
    sdl::main::SetMainReady();
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
                                           try
                                           {
                                               EngineLifecycle engine_lifecycle{config};
                                               auto &engine = Engine::instance();
                                               engine.run();
                                           }
                                           catch (const std::exception &ex)
                                           {
                                               std::cerr << "Fatal error: " << ex.what() << '\n';
                                           }
                                           game_thread_exited.store(true);
                                       }};

        while (!game_thread_exited.load())
        {
            sdl::Event event;
            while (sdl::WaitEventTimeout(&event, 10))
            {
                switch (static_cast<sdl::EventType>(event.type))
                {
                    case sdl::EventType::QUIT:
                    case sdl::EventType::WINDOW_CLOSE_REQUESTED:
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