/**
 * @file input_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.input.input_manager;

import std;
import retro.platform.input;
import retro.platform.event;
import retro.core.math.vector;
import retro.runtime.input.input_state;
import retro.core.containers.optional;
import retro.runtime.input.input_query;
import retro.core.type_traits.variant;
import retro.core.functional.overload;

namespace retro
{
    export using InputEvent =
        std::variant<MouseMovedEvent, MouseButtonEvent, MouseWheelEvent, KeyEvent, GamepadDeviceEvent>;

    export class RETRO_API InputManager
    {
      public:
        void push_event(const InputEvent &event);

        void poll_events(std::uint64_t frame_count);

        [[nodiscard]] constexpr bool is_down(const ButtonInput input) const
        {
            return std::visit(Overload{[this](const LogicalKey key) { return current_.is_down(key); },
                                       [this](const PhysicalKey key) { return current_.is_down(key); },
                                       [this](const MouseButton button) { return current_.is_down(button); },
                                       [this](const GamepadButtonInput button)
                                       { return current_.is_down(button.gamepad_index, button.button); },
                                       [this](const AnyGamepadButtonInput button)
                                       {
                                           return current_.is_down_on_any(button.button);
                                       }},
                              input);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool is_any_down(Range &&range) const
        {
            return std::ranges::any_of(std::forward<Range>(range),
                                       [this](const ButtonInput button) { return is_down(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool are_all_down(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range),
                                       [this](const ButtonInput button) { return is_down(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool are_none_down(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range),
                                        [this](const ButtonInput button) { return is_down(button); });
        }

        [[nodiscard]] constexpr bool was_pressed(const ButtonInput input) const
        {
            return std::visit(Overload{[this](const LogicalKey key) { return current_.was_pressed(key, previous_); },
                                       [this](const PhysicalKey key) { return current_.was_pressed(key, previous_); },
                                       [this](const MouseButton button)
                                       { return current_.was_pressed(button, previous_); },
                                       [this](const GamepadButtonInput button)
                                       { return current_.was_pressed(button.gamepad_index, button.button, previous_); },
                                       [this](const AnyGamepadButtonInput button)
                                       {
                                           return current_.was_pressed_on_any(button.button, previous_);
                                       }},
                              input);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool was_any_pressed(Range &&range) const
        {
            return std::ranges::any_of(std::forward<Range>(range),
                                       [this](const ButtonInput button) { return was_pressed(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool were_all_pressed(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range),
                                       [this](const ButtonInput button) { return was_pressed(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ButtonInput>
        [[nodiscard]] constexpr bool were_none_pressed(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range),
                                        [this](const ButtonInput button) { return was_pressed(button); });
        }

        [[nodiscard]] constexpr Vector2f mouse_position() const
        {
            return current_.mouse_position;
        }

        [[nodiscard]] constexpr Vector2f mouse_delta() const
        {
            return current_.mouse_delta;
        }

        [[nodiscard]] constexpr Vector2f mouse_wheel_delta() const
        {
            return current_.mouse_wheel_delta;
        }

      private:
        [[nodiscard]] std::uint8_t find_gamepad_mapping(std::uint32_t platform_id) const;

        std::uint8_t add_gamepad_mapping(std::uint32_t platform_id);

        bool remove_gamepad_mapping(std::uint32_t platform_id);

        void apply_event(const InputEvent &event);

        std::mutex mutex_;
        std::vector<InputEvent> event_write_queue_;
        std::vector<InputEvent> event_read_queue_;

        std::array<Optional<std::uint32_t>, max_gamepads> gamepad_mappings_;

        InputState state_;
        InputSnapshot previous_;
        InputSnapshot current_;
    };
} // namespace retro
