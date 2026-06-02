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
    enum class DigitalInputType : std::uint8_t
    {
        logical,
        physical,
        mouse,
        gamepad,
        any_gamepad,
        gamepad_axis_threshold,
        any_gamepad_axis_threshold
    };

    struct InteropDigitalInput
    {
        DigitalInputType type = DigitalInputType::logical;

        union
        {
            LogicalKey logical_key = LogicalKey::unknown;
            PhysicalKey physical_key;
            MouseButton mouse_button;
            GamepadButtonInput gamepad_button;
            AnyGamepadButtonInput any_gamepad_button;
            GamepadAxisThreshold gamepad_axis_threshold;
            AnyGamepadAxisThreshold any_gamepad_axis_threshold;
        };

        constexpr explicit(false) operator DigitalInput() const noexcept
        {
            switch (type)
            {
                case DigitalInputType::logical:
                    return logical_key;
                case DigitalInputType::physical:
                    return physical_key;
                case DigitalInputType::mouse:
                    return mouse_button;
                case DigitalInputType::gamepad:
                    return gamepad_button;
                case DigitalInputType::any_gamepad:
                    return any_gamepad_button;
                case DigitalInputType::gamepad_axis_threshold:
                    return gamepad_axis_threshold;
                case DigitalInputType::any_gamepad_axis_threshold:
                    return any_gamepad_axis_threshold;
            }

            return {};
        }
    };

    enum class AnalogueInputType : std::uint8_t
    {
        logical,
        physical,
        mouse,
        gamepad,
        any_gamepad,
        gamepad_axis,
        any_gamepad_axis
    };

    struct InteropAnalogueInput
    {
        AnalogueInputType type = AnalogueInputType::logical;
        union
        {
            LogicalKeyAnalogueInput logical_key{};
            PhysicalKeyAnalogueInput physical_key;
            MouseButtonAnalogueInput mouse_button;
            GamepadButtonAnalogueInput gamepad_button;
            AnyGamepadButtonAnalogueInput any_gamepad_button;
            GamepadAxisInput gamepad_axis;
            AnyGamepadAxisInput any_gamepad_axis;
        };

        constexpr explicit(false) operator AnalogueInput() const noexcept
        {
            switch (type)
            {
                case AnalogueInputType::logical:
                    return logical_key;
                case AnalogueInputType::physical:
                    return physical_key;
                case AnalogueInputType::mouse:
                    return mouse_button;
                case AnalogueInputType::gamepad:
                    return gamepad_button;
                case AnalogueInputType::any_gamepad:
                    return any_gamepad_button;
                case AnalogueInputType::gamepad_axis:
                    return gamepad_axis;
                case AnalogueInputType::any_gamepad_axis:
                    return any_gamepad_axis;
            }
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

    RETRO_API bool retro_input_manager_is_down(const InputManager *manager, const InteropDigitalInput input)
    {
        return manager->is_down(input);
    }

    RETRO_API bool retro_input_manager_is_any_down(const InputManager *manager,
                                                   const InteropDigitalInput *inputs,
                                                   const std::int32_t length)
    {
        return manager->is_any_down(std::span{inputs, static_cast<std::size_t>(length)});
    }

    RETRO_API bool retro_input_manager_are_all_down(const InputManager *manager,
                                                    const InteropDigitalInput *inputs,
                                                    const std::int32_t length)
    {
        return manager->are_all_down(std::span{inputs, static_cast<std::size_t>(length)});
    }

    RETRO_API bool retro_input_manager_are_none_down(const InputManager *manager,
                                                     const InteropDigitalInput *inputs,
                                                     const std::int32_t length)
    {
        return manager->are_none_down(std::span{inputs, static_cast<std::size_t>(length)});
    }

    RETRO_API bool retro_input_manager_was_pressed(const InputManager *manager, const InteropDigitalInput input)
    {
        return manager->was_pressed(input);
    }

    RETRO_API bool retro_input_manager_was_any_pressed(const InputManager *manager,
                                                       const InteropDigitalInput *inputs,
                                                       const std::int32_t length)
    {
        return manager->was_any_pressed(std::span{inputs, static_cast<std::size_t>(length)});
    }

    RETRO_API bool retro_input_manager_were_all_pressed(const InputManager *manager,
                                                        const InteropDigitalInput *inputs,
                                                        const std::int32_t length)
    {
        return manager->were_all_pressed(std::span{inputs, static_cast<std::size_t>(length)});
    }

    RETRO_API bool retro_input_manager_wre_none_pressed(const InputManager *manager,
                                                        const InteropDigitalInput *inputs,
                                                        const std::int32_t length)
    {
        return manager->were_none_pressed(std::span{inputs, static_cast<std::size_t>(length)});
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

    RETRO_API float retro_input_manager_get_analogue_value(const InputManager *manager,
                                                           const InteropAnalogueInput input)
    {
        return manager->get_analogue_value(input);
    }

    RETRO_API float retro_input_manager_get_analogue_values(const InputManager *manager,
                                                            const InteropAnalogueInput *inputs,
                                                            const std::int32_t count)
    {
        return manager->get_analogue_values(std::span{inputs, static_cast<std::size_t>(count)});
    }
}
