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

using namespace retro;

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

    RETRO_API bool retro_input_manager_is_logical_key_down(const InputManager *manager, const LogicalKey key)
    {
        return manager->is_down(key);
    }

    RETRO_API bool retro_input_manager_is_physical_key_down(const InputManager *manager, const PhysicalKey key)
    {
        return manager->is_down(key);
    }

    RETRO_API bool retro_input_manager_is_mouse_button_down(const InputManager *manager, const MouseButton button)
    {
        return manager->is_down(button);
    }

    RETRO_API bool retro_input_manager_is_any_down(const InputManager *manager,
                                                   const LogicalKey *logical_keys,
                                                   const std::int32_t logical_key_count,
                                                   const PhysicalKey *physical_keys,
                                                   const std::int32_t physical_key_count,
                                                   const MouseButton *mouse_buttons,
                                                   const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->is_any_down(logical_key_span) || manager->is_any_down(physical_key_span) ||
               manager->is_any_down(mouse_button_span);
    }

    RETRO_API bool retro_input_manager_are_all_down(const InputManager *manager,
                                                    const LogicalKey *logical_keys,
                                                    const std::int32_t logical_key_count,
                                                    const PhysicalKey *physical_keys,
                                                    const std::int32_t physical_key_count,
                                                    const MouseButton *mouse_buttons,
                                                    const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->are_all_down(logical_key_span) || manager->are_all_down(physical_key_span) ||
               manager->are_all_down(mouse_button_span);
    }

    RETRO_API bool retro_input_manager_are_none_down(const InputManager *manager,
                                                     const LogicalKey *logical_keys,
                                                     const std::int32_t logical_key_count,
                                                     const PhysicalKey *physical_keys,
                                                     const std::int32_t physical_key_count,
                                                     const MouseButton *mouse_buttons,
                                                     const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->are_none_down(logical_key_span) || manager->are_none_down(physical_key_span) ||
               manager->are_none_down(mouse_button_span);
    }

    RETRO_API bool retro_input_manager_was_logical_key_pressed(const InputManager *manager, const LogicalKey key)
    {
        return manager->was_pressed(key);
    }

    RETRO_API bool retro_input_manager_was_physical_key_pressed(const InputManager *manager, const PhysicalKey key)
    {
        return manager->was_pressed(key);
    }

    RETRO_API bool retro_input_manager_was_mouse_button_pressed(const InputManager *manager, const MouseButton button)
    {
        return manager->was_pressed(button);
    }

    RETRO_API bool retro_input_manager_were_any_pressed(const InputManager *manager,
                                                        const LogicalKey *logical_keys,
                                                        const std::int32_t logical_key_count,
                                                        const PhysicalKey *physical_keys,
                                                        const std::int32_t physical_key_count,
                                                        const MouseButton *mouse_buttons,
                                                        const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->was_any_pressed(logical_key_span) || manager->was_any_pressed(physical_key_span) ||
               manager->was_any_pressed(mouse_button_span);
    }

    RETRO_API bool retro_input_manager_were_all_pressed(const InputManager *manager,
                                                        const LogicalKey *logical_keys,
                                                        const std::int32_t logical_key_count,
                                                        const PhysicalKey *physical_keys,
                                                        const std::int32_t physical_key_count,
                                                        const MouseButton *mouse_buttons,
                                                        const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->were_all_pressed(logical_key_span) || manager->were_all_pressed(physical_key_span) ||
               manager->were_all_pressed(mouse_button_span);
    }

    RETRO_API bool retro_input_manager_were_none_pressed(const InputManager *manager,
                                                         const LogicalKey *logical_keys,
                                                         const std::int32_t logical_key_count,
                                                         const PhysicalKey *physical_keys,
                                                         const std::int32_t physical_key_count,
                                                         const MouseButton *mouse_buttons,
                                                         const std::int32_t mouse_button_count)
    {
        std::span logical_key_span{logical_keys, static_cast<std::size_t>(logical_key_count)};
        std::span physical_key_span{physical_keys, static_cast<std::size_t>(physical_key_count)};
        std::span mouse_button_span{mouse_buttons, static_cast<std::size_t>(mouse_button_count)};

        return manager->were_none_pressed(logical_key_span) || manager->were_none_pressed(physical_key_span) ||
               manager->were_none_pressed(mouse_button_span);
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
