/**
 * @file main.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import retro.core.di;
import retro.runtime;
import retro.scripting;
import retro.renderer;
import retro.logging;
import retro.platform;
import std;

int main()
{
    using namespace retro;

    init_logger();

    PlatformContext sdl_runtime{PlatformInitFlags::Video};

    try
    {
        const auto window = Window::create_shared({.flags = WindowFlags::Resizable | WindowFlags::Vulkan});
        ServiceCollection service_collection;
        add_engine_services(service_collection);
        add_rendering_services(service_collection, window);
        add_scripting_services(service_collection);

        ServiceProvider service_provider{service_collection};

        std::atomic game_thread_exited = false;
        auto game_thread = std::thread{
            [&]
            {
                try
                {
                    EngineLifecycle engine_lifecycle{service_provider.create<std::unique_ptr<Engine>>()};
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
            while (auto event = wait_for_event(std::chrono::milliseconds(10)))
            {
                std::visit(
                    [&]<typename T>(const T &)
                    {
                        if constexpr (std::is_same_v<T, QuitEvent> || std::is_same_v<T, WindowCloseRequestedEvent>)
                        {
                            if (!game_thread_exited.load())
                            {
                                Engine::instance().request_shutdown();
                            }
                        }
                    },
                    *event);

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
