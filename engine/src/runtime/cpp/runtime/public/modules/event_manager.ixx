/**
 * @file event_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.event_manager;

import std;
import retro.platform.event;
import retro.core.functional.delegate;

namespace retro
{
    export using EngineEvent = std::variant<WindowResizedEvent>;

    export using WindowResizedDelegate = MulticastDelegate<void(std::uint64_t, std::uint32_t, std::uint32_t)>;

    export class RETRO_API EventManager
    {
      public:
        void push_event(const EngineEvent &event);

        void poll_events();

        inline WindowResizedDelegate::Event window_resized()
        {
            return window_resized_;
        }

      private:
        std::vector<EngineEvent> pending_events_;
        std::vector<EngineEvent> polling_events_;
        std::mutex event_mutex_;

        WindowResizedDelegate window_resized_;
    };
} // namespace retro
