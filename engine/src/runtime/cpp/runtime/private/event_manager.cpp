/**
 * @file event_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.event_manager;
import retro.core.functional.overload;

namespace retro
{

    void EventManager::push_event(const EngineEvent &event)
    {
        std::scoped_lock lock{event_mutex_};
        pending_events_.push_back(event);
    }

    void EventManager::poll_events()
    {
        {
            std::scoped_lock lock{event_mutex_};
            polling_events_.swap(pending_events_);
        }

        for (auto &event : polling_events_)
        {
            std::visit(Overload{[this](const WindowResizedEvent &e)
                                {
                                    window_resized_.broadcast(e.window_id, e.width, e.height);
                                }},
                       event);
        }

        polling_events_.clear();
    }
} // namespace retro
