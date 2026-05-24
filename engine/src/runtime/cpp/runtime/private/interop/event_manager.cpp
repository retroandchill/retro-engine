/**
 * @file event_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <memory>

import retro.runtime.event_manager;
import retro.platform.event;
import retro.core.functional.interop_function;

using namespace retro;

namespace
{
    using DeleteCallback = void (*)(void *);
    using EqualsCallback = bool (*)(void *, void *);
    using WindowResizedCallback = void (*)(void *, std::uint64_t, std::uint32_t, std::uint32_t);
} // namespace

extern "C"
{
    RETRO_API EventManager *retro_event_manager_create()
    {
        return new EventManager();
    }

    RETRO_API void retro_event_manager_destroy(const EventManager *manager)
    {
        delete manager;
    }

    RETRO_API void retro_event_manager_poll_events(EventManager *manager)
    {
        manager->poll_events();
    }

    RETRO_API void retro_event_manager_window_resized_add(EventManager *manager,
                                                          void *user_data,
                                                          WindowResizedCallback callback,
                                                          DeleteCallback delete_callback,
                                                          EqualsCallback equals_callback)
    {
        InteropFunction function{callback,
                                 std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                 equals_callback};
        manager->window_resized().add(std::move(function));
    }

    RETRO_API void retro_event_manager_window_resized_remove(EventManager *manager,
                                                             void *user_data,
                                                             const WindowResizedCallback callback,
                                                             const DeleteCallback delete_callback,
                                                             const EqualsCallback equals_callback)
    {
        InteropFunction function{callback,
                                 std::unique_ptr<void, DeleteCallback>{user_data, delete_callback},
                                 equals_callback};
        manager->window_resized().remove(std::move(function));
    }
}
