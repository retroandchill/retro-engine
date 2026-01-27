/**
 * @file main.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
import retro.core;
import retro.runtime;
import retro.scripting;
import retro.renderer;
import retro.logging;
import std;
import sdl;

int main()
{
    using namespace retro;

    init_logger();

    sdl::main::SetMainReady();
    SdlRuntime sdl_runtime;

    try
    {
        const auto window = std::make_shared<Window>(1280, 720, "Retro Engine");
        const auto injector = boost::di::make_injector(make_scripting_injector(),
                                                       make_rendering_injector(window),
                                                       make_runtime_injector());

        std::atomic game_thread_exited = false;
        auto game_thread = std::thread{
            [&]
            {
                try
                {
                    EngineLifecycle engine_lifecycle{injector.create<std::unique_ptr<Engine>>()};
                    auto &engine = Engine::instance();
                    engine.run(u"RetroEngine.Game.Sample.dll", u"RetroEngine.Game.Sample.GameRunner", u"Main");
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
        get_logger().critical("Fatal error: {}", ex.what());
        return -1;
    }
}
