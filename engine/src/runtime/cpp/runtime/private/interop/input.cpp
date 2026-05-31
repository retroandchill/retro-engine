/**
 * @file input.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.platform.input;
import retro.runtime.input.input_manager;
import retro.runtime.input.input_query;

using namespace retro;

namespace
{
    enum class ButtonInputType : std::uint8_t
    {
        logical,
        physical,
        mouse,
        gamepad,
        any_gamepad
    };

    struct InteropButtonInput
    {
        ButtonInputType type = ButtonInputType::logical;

        union
        {
            LogicalKey logical_key = LogicalKey::unknown;
            PhysicalKey physical_key;
            MouseButton mouse_button;
            GamepadButtonInput gamepad_button;
            AnyGamepadButtonInput any_gamepad_button;
        };

        constexpr explicit(false) operator ButtonInput() const noexcept
        {
            switch (type)
            {
                case ButtonInputType::logical:
                    return logical_key;
                case ButtonInputType::physical:
                    return physical_key;
                case ButtonInputType::mouse:
                    return mouse_button;
                case ButtonInputType::gamepad:
                    return gamepad_button;
                case ButtonInputType::any_gamepad:
                    return any_gamepad_button;
            }

            return {};
        }
    };
} // namespace

extern "C"
{
    RETRO_API InputManager *retro_input_manager_create()
    {
        return new InputManager();
    }

    RETRO_API void retro_input_manager_destroy(const InputManager *manager)
    {
        delete manager;
    }

    RETRO_API void retro_input_manager_poll_events(InputManager *manager, const std::uint64_t frame_number)
    {
        manager->poll_events(frame_number);
    }

    RETRO_API bool retro_input_manager_is_logical_key_down(const InputManager *manager, const InteropButtonInput input)
    {
        return manager->is_down(input);
    }

    RETRO_API bool retro_input_manager_was_logical_key_pressed(const InputManager *manager,
                                                               const InteropButtonInput input)
    {
        return manager->was_pressed(input);
    }

    RETRO_API void retro_input_manager_get_mouse_position(const InputManager *manager,
                                                          float *position_x,
                                                          float *position_y,
                                                          float *position_delta_x,
                                                          float *position_delta_y,
                                                          float *scroll_delta_x,
                                                          float *scroll_delta_y)
    {
        *position_x = manager->mouse_position().x;
        *position_y = manager->mouse_position().y;
        *position_delta_x = manager->mouse_delta().x;
        *position_delta_y = manager->mouse_delta().y;
        *scroll_delta_x = manager->mouse_wheel_delta().x;
        *scroll_delta_y = manager->mouse_wheel_delta().y;
    }
}
