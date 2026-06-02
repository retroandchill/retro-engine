/**
 * @file input_query.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.input.input_query;

import std;
import retro.platform.input;

namespace retro
{
    export struct GamepadButtonInput
    {
        std::uint8_t gamepad_index = 0;
        GamepadButton button = GamepadButton::unknown;
    };

    export struct AnyGamepadButtonInput
    {
        GamepadButton button = GamepadButton::unknown;
    };

    export struct GamepadAxisThreshold
    {
        std::uint8_t gamepad_index = 0;
        GamepadAxis axis = GamepadAxis::unknown;
        float threshold = 0.0f;
    };

    export struct AnyGamepadAxisThreshold
    {
        GamepadAxis axis = GamepadAxis::unknown;
        float threshold = 0.0f;
    };

    export using DigitalInput = std::variant<LogicalKey,
                                             PhysicalKey,
                                             MouseButton,
                                             GamepadButtonInput,
                                             AnyGamepadButtonInput,
                                             GamepadAxisThreshold,
                                             AnyGamepadAxisThreshold>;

    export struct LogicalKeyAnalogueInput
    {
        LogicalKey logical_key = LogicalKey::unknown;
        float axis_value = 0.0f;
    };

    export struct PhysicalKeyAnalogueInput
    {
        PhysicalKey physical_key = PhysicalKey::unknown;
        float axis_value = 0.0f;
    };

    export struct MouseButtonAnalogueInput
    {
        MouseButton mouse_button = MouseButton::unknown;
        float axis_value = 0.0f;
    };

    export struct GamepadButtonAnalogueInput
    {
        std::uint8_t gamepad_index = 0;
        GamepadButton gamepad_button = GamepadButton::unknown;
        float axis_value = 0.0f;
    };

    export struct AnyGamepadButtonAnalogueInput
    {
        GamepadButton gamepad_button = GamepadButton::unknown;
        float axis_value = 0.0f;
    };

    export struct GamepadAxisInput
    {
        std::uint8_t gamepad_index = 0;
        GamepadAxis axis = GamepadAxis::unknown;
        bool invert = false;
    };

    export struct AnyGamepadAxisInput
    {
        GamepadAxis axis = GamepadAxis::unknown;
        bool invert = false;
    };

    export using AnalogueInput = std::variant<LogicalKeyAnalogueInput,
                                              PhysicalKeyAnalogueInput,
                                              MouseButtonAnalogueInput,
                                              GamepadButtonAnalogueInput,
                                              AnyGamepadButtonAnalogueInput,
                                              GamepadAxisInput,
                                              AnyGamepadAxisInput>;
} // namespace retro
