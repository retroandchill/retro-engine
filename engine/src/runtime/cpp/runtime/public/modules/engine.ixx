/**
 * @file engine.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.engine;

import std;
import retro.core.async.task;
import retro.core.async.manual_task_scheduler;
import retro.core.functional.delegate;
import retro.runtime.rendering.renderer2d;
import retro.core.memory.ref_counted_ptr;
import retro.platform.window;
import retro.core.containers.optional;
import retro.runtime.world.scene;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.world.viewport;
import retro.platform.backend;
import retro.platform.event;
import retro.core.functional.function_ref;
import retro.core.type_traits.range;

namespace retro
{
    export using OnWindowCloseRequested = MulticastDelegate<void(std::uint64_t)>;
    export using OnShutdownRequested = MulticastDelegate<void()>;

    export class Engine
    {
      public:
        RETRO_API explicit Engine(PlatformBackend &platform_backend);

        ~Engine() = default;

        Engine(const Engine &) = delete;
        Engine(Engine &&) noexcept = delete;
        Engine &operator=(const Engine &) = delete;
        Engine &operator=(Engine &&) noexcept = delete;

        RETRO_API void wait_platform_event(std::chrono::milliseconds timeout);

        RETRO_API void poll_events_once();

        inline OnWindowCloseRequested::Event on_window_close_requested()
        {
            return on_window_close_requested_;
        }

        inline OnShutdownRequested::Event on_shutdown_requested()
        {
            return on_shutdown_requested_;
        }

      private:
        bool handle_platform_event(const Event &event);

        PlatformBackend &platform_backend_;
        OnWindowCloseRequested on_window_close_requested_;
        OnShutdownRequested on_shutdown_requested_;
    };
} // namespace retro
