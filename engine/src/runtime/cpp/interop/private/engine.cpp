/**
 * @file engine.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.engine;
import retro.core.functional.interop_function;
import retro.core.async.task;
import retro.core.strings.encoding;
import retro.platform.backend;
import retro.platform.window;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.interop.interop_error;
import retro.runtime.rendering.pipeline_manager.render_manager;

namespace
{
    using DeleteCallback = void (*)(void *);
    using EqualsCallback = bool (*)(void *, void *);
    using ShutdownRequestedCallback = void (*)(void *);
} // namespace

extern "C"
{
    RETRO_API retro::Engine *retro_create_engine(retro::PlatformBackend *backend)
    {
        return new retro::Engine(*backend);
    }

    RETRO_API void retro_destroy_engine(const retro::Engine *engine)
    {
        delete engine;
    }

    RETRO_API void retro_engine_wait_platform_events(retro::Engine *engine, std::int64_t timeout)
    {
        engine->wait_platform_event(std::chrono::milliseconds{timeout});
    }

    RETRO_API void retro_engine_poll_platform_events(retro::Engine *engine)
    {
        engine->poll_events_once();
    }

    RETRO_API void retro_engine_on_shutdown_requested_add(retro::Engine *engine,
                                                          void *user_data,
                                                          const ShutdownRequestedCallback removed_callback,
                                                          const DeleteCallback delete_callback,
                                                          const EqualsCallback equals_callback)
    {
        retro::InteropFunction<void()> function{removed_callback,
                                                std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                                equals_callback};
        engine->on_shutdown_requested().add(std::move(function));
    }

    RETRO_API void retro_bind_window_close_events(retro::Engine *engine, retro::RenderManager *render_manager)
    {
        engine->on_window_close_requested().add([render_manager](const std::uint64_t id)
                                                { render_manager->remove_window(id); });
    }
}
