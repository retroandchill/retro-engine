/**
 * @file main.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import retro.core.di;
import retro.runtime.engine;
import retro.scripting.services;
import retro.renderer.services;
import retro.logging;
import retro.platform.backend;
import retro.platform.event;
import retro.platform.window;
import std;

int main()
{
    using namespace retro;

    init_logger();

    const auto platform_backend =
        PlatformBackend::create({.kind = PlatformBackendKind::sdl3, .flags = PlatformInitFlags::video});

    try
    {
        const auto window = platform_backend->create_window({.flags = WindowFlags::resizable | WindowFlags::vulkan});
        ServiceCollection service_collection;
        service_collection.add(*platform_backend);
        add_engine_services(service_collection);
        add_rendering_services(service_collection, window);
        add_scripting_services(service_collection);

        const auto service_provider = service_collection.create_service_provider();

        std::atomic game_thread_exited = false;
        auto game_thread = std::thread{
            [&]
            {
                try
                {
                    EngineLifecycle engine_lifecycle{service_provider->get_required<Engine>()};
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
            while (auto event = platform_backend->wait_for_event(std::chrono::milliseconds(10)))
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
