/**
 * @file event.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.event;

import retro.core.containers.optional;
import std;
import retro.platform.input;

namespace retro
{
    export struct QuitEvent
    {
    };

    export struct WindowCloseRequestedEvent
    {
        std::uint32_t window_id = 0;
    };

    export struct WindowResizedEvent
    {
        std::uint32_t window_id = 0;
        std::int32_t width = 0;
        std::int32_t height = 0;
    };

    export struct MouseMovedEvent
    {
        std::uint32_t window_id = 0;
        float x = 0.0f;
        float y = 0.0f;
        float dx = 0.0f;
        float dy = 0.0f;
    };

    export struct MouseButtonEvent
    {
        std::uint32_t window_id = 0;
        MouseButton button = MouseButton::unknown;
        bool down = false;
        float x = 0.0f;
        float y = 0.0f;
    };

    export struct MouseWheelEvent
    {
        std::uint32_t window_id = 0;
        float x = 0.0f;
        float y = 0.0f;
        MouseWheelDirection direction = MouseWheelDirection::normal;
        float mouse_x = 0.0f;
        float mouse_y = 0.0f;
        std::int32_t mouse_ticks_x = 0;
        std::int32_t mouse_ticks_y = 0;
    };

    export struct KeyEvent
    {
        std::uint32_t window_id = 0;
        LogicalKey logical_key = LogicalKey::unknown;
        PhysicalKey physical_key = PhysicalKey::unknown;
        bool down = false;
        bool repeat = false;
    };

    export struct GamepadDeviceEvent
    {
        GamepadChangeType change_type = GamepadChangeType::connected;
        std::uint32_t gamepad_id = 0;
    };

    export struct GamepadButtonEvent
    {
        std::uint32_t gamepad_id = 0;
        GamepadButton button = GamepadButton::unknown;
        bool down = false;
    };

    export struct GamepadAxisEvent
    {
        std::uint32_t gamepad_id = 0;
        GamepadAxis axis = GamepadAxis::unknown;
        float value = 0.0f;
    };

    export struct CallbackEvent
    {
        std::function<void()> callback;
    };

    export using PlatformEvent = std::variant<QuitEvent,
                                              WindowCloseRequestedEvent,
                                              WindowResizedEvent,
                                              MouseMovedEvent,
                                              MouseButtonEvent,
                                              MouseWheelEvent,
                                              KeyEvent,
                                              GamepadDeviceEvent,
                                              GamepadButtonEvent,
                                              GamepadAxisEvent,
                                              CallbackEvent>;
} // namespace retro
