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

namespace retro
{
    export using InputEvent = std::variant<MouseMovedEvent, MouseButtonEvent, MouseWheelEvent, KeyEvent>;

    template <typename T>
    concept ValidButtonInput = std::convertible_to<T, LogicalKey> || std::convertible_to<T, PhysicalKey> ||
                               std::convertible_to<T, MouseButton>;

    export class RETRO_API InputManager
    {
      public:
        void push_event(const InputEvent &event);

        void poll_events(std::uint64_t frame_count);

        [[nodiscard]] constexpr bool is_down(const LogicalKey key) const
        {
            return current_.is_down(key);
        }

        [[nodiscard]] constexpr bool is_down(const PhysicalKey key) const
        {
            return current_.is_down(key);
        }

        [[nodiscard]] constexpr bool is_down(const MouseButton button) const
        {
            return current_.is_down(button);
        }

        [[nodiscard]] constexpr bool is_any_down() const
        {
            return current_.logical_keys.any() || current_.physical_keys.any() || current_.mouse_buttons.any();
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool is_any_down(Range &&keys) const
        {
            return std::ranges::any_of(std::forward<Range>(keys), [&](auto key) { return is_down(key); });
        }

        [[nodiscard]] constexpr bool is_any_logical_key_down() const
        {
            return current_.logical_keys.any();
        }

        [[nodiscard]] constexpr bool is_any_physical_key_down() const
        {
            return current_.physical_keys.any();
        }

        [[nodiscard]] constexpr bool is_any_mouse_button_down() const
        {
            return current_.mouse_buttons.any();
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool are_all_down(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range), [&](auto key) { return is_down(key); });
        }

        [[nodiscard]] constexpr bool are_none_down() const
        {
            return current_.logical_keys.none() && current_.physical_keys.none() && current_.mouse_buttons.none();
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool are_none_down(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range), [&](auto key) { return is_down(key); });
        }

        [[nodiscard]] constexpr bool was_pressed(const LogicalKey key) const
        {
            return current_.was_pressed(key, previous_);
        }

        [[nodiscard]] constexpr bool was_pressed(const PhysicalKey key) const
        {
            return current_.was_pressed(key, previous_);
        }

        [[nodiscard]] constexpr bool was_pressed(const MouseButton button) const
        {
            return current_.was_pressed(button, previous_);
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool was_any_pressed(Range &&keys) const
        {
            return std::ranges::any_of(std::forward<Range>(keys), [&](auto key) { return was_pressed(key); });
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool were_all_pressed(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range), [&](auto key) { return was_pressed(key); });
        }

        template <std::ranges::input_range Range>
            requires ValidButtonInput<std::ranges::range_value_t<Range>>
        [[nodiscard]] constexpr bool were_none_pressed(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range), [&](auto key) { return was_pressed(key); });
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
        void apply_event(const InputEvent &event);

        std::mutex mutex_;
        std::vector<InputEvent> event_write_queue_;
        std::vector<InputEvent> event_read_queue_;

        InputState state_;
        InputSnapshot previous_;
        InputSnapshot current_;
    };
} // namespace retro
