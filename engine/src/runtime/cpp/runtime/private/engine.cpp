/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.runtime.engine;

import retro.logging;
import retro.core.async.task_scheduler;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.core.containers.optional;
import retro.core.math.vector;
import retro.platform.event;
import retro.core.memory.small_unique_ptr;
import retro.runtime.rendering.draw_command;

namespace retro
{

    Engine::Engine(PlatformBackend &platform_backend) : platform_backend_{platform_backend}
    {
    }

    void Engine::wait_platform_event(const std::chrono::milliseconds timeout)
    {
        if (auto event = platform_backend_.wait_for_event(timeout))
        {
            handle_platform_event(*event);
        }
    }

    void Engine::poll_events_once()
    {
        while (auto event = platform_backend_.poll_event())
        {
            if (!handle_platform_event(*event))
            {
                // ReSharper disable once CppDFAUnreachableCode
                break;
            }
        }
    }

    bool Engine::handle_platform_event(const Event &event)
    {
        return std::visit(
            [&]<typename T>(const T &evt)
            {
                if constexpr (std::is_same_v<T, QuitEvent>)
                {
                    on_shutdown_requested_();
                    return false;
                }
                else if constexpr (std::is_same_v<T, WindowCloseRequestedEvent>)
                {
                    on_window_close_requested_(evt.window_id);
                }
                else if constexpr (std::is_same_v<T, CallbackEvent>)
                {
                    if (evt.callback)
                    {
                        evt.callback();
                    }
                }

                return true;
            },
            event);
    }
} // namespace retro
