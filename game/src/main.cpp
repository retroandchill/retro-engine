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

void set_up_test_scene(const retro::Engine &engine);

int main()
{
    using namespace retro;

    init_logger();

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
                                               engine.run([&] { set_up_test_scene(engine); });
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

void set_up_test_scene(const retro::Engine &engine)
{
    using namespace retro;

    constexpr int width = 1280 / 100 + 1;
    constexpr int height = 720 / 100 + 1;
    int count = 0;
    for (int i = 0; i < width; i++)
    {
        for (int j = 0; j < height; j++)
        {
            const int index = i + j * width;           // linear index if needed
            const float r = (index & 1) ? 1.0f : 0.0f; // bit 0
            const float g = (index & 2) ? 1.0f : 0.0f; // bit 1
            const float b = (index & 4) ? 1.0f : 0.0f; // bit 2

            const Color c{r, g, b, 1.0f};
            auto &entity = engine.scene().create_entity();
            entity.set_position({static_cast<float>(i) * 100.0f, static_cast<float>(j) * 100.0f});
            auto &component = engine.scene().create_component<QuadRenderComponent>(entity.id());
            component.set_size({100.0f, 100.0f});
            component.set_color(c);

            count++;
        }
    }
}
