/**
 * @file events.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.platform:events;

import retro.core;
import std;

namespace retro
{
    export struct QuitEvent
    {
    };

    export struct WindowCloseRequestedEvent
    {
        uint32 window_id = 0;
    };

    export struct WindowResizedEvent
    {
        uint32 window_id = 0;
        int32 width = 0;
        int32 height = 0;
    };

    export enum class MouseButton : uint8
    {
        Left,
        Middle,
        Right,
        X1,
        X2,
        Unknown,
    };

    export struct MouseMovedEvent
    {
        uint32 window_id = 0;
        float x = 0.0f;
        float y = 0.0f;
        float dx = 0.0f;
        float dy = 0.0f;
    };

    export struct MouseButtonEvent
    {
        uint32 window_id = 0;
        MouseButton button = MouseButton::Unknown;
        bool down = false;
        float x = 0.0f;
        float y = 0.0f;
    };

    export struct KeyEvent
    {
        uint32 window_id = 0;
        int32 keycode = 0;
        int32 scancode = 0;
        bool down = false;
        bool repeat = false;
    };

    export using Event = std::
        variant<QuitEvent, WindowCloseRequestedEvent, WindowResizedEvent, MouseMovedEvent, MouseButtonEvent, KeyEvent>;

    export RETRO_API Optional<Event> poll_event();

    export RETRO_API Optional<Event> wait_for_event();

    export RETRO_API Optional<Event> wait_for_event(std::chrono::milliseconds timeout);
} // namespace retro
