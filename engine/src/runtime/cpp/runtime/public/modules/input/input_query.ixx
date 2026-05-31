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

    export using ButtonInput =
        std::variant<LogicalKey, PhysicalKey, MouseButton, GamepadButtonInput, AnyGamepadButtonInput>;
} // namespace retro
