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

        [[nodiscard]] constexpr bool is_down(const DigitalInput input) const
        {
            return std::visit(
                Overload{[this](const LogicalKey key) { return current_.is_down(key); },
                         [this](const PhysicalKey key) { return current_.is_down(key); },
                         [this](const MouseButton button) { return current_.is_down(button); },
                         [this](const GamepadButtonInput button)
                         { return current_.is_down(button.gamepad_index, button.button); },
                         [this](const AnyGamepadButtonInput button) { return current_.is_down_on_any(button.button); },
                         [this](const GamepadAxisThreshold &threshold) {
                             return current_.axis_over_threshold(threshold.gamepad_index,
                                                                 threshold.axis,
                                                                 threshold.threshold);
                         },
                         [this](const AnyGamepadAxisThreshold &threshold)
                         {
                             return current_.axis_over_threshold_on_any(threshold.axis, threshold.threshold);
                         }},
                input);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool is_any_down(Range &&range) const
        {
            return std::ranges::any_of(std::forward<Range>(range),
                                       [this](const DigitalInput button) { return is_down(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool are_all_down(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range),
                                       [this](const DigitalInput button) { return is_down(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool are_none_down(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range),
                                        [this](const DigitalInput button) { return is_down(button); });
        }

        [[nodiscard]] constexpr bool was_pressed(const DigitalInput input) const
        {
            return std::visit(Overload{[this](const LogicalKey key) { return current_.was_pressed(key, previous_); },
                                       [this](const PhysicalKey key) { return current_.was_pressed(key, previous_); },
                                       [this](const MouseButton button)
                                       { return current_.was_pressed(button, previous_); },
                                       [this](const GamepadButtonInput button)
                                       { return current_.was_pressed(button.gamepad_index, button.button, previous_); },
                                       [this](const AnyGamepadButtonInput button)
                                       { return current_.was_pressed_on_any(button.button, previous_); },
                                       [this](const GamepadAxisThreshold &threshold)
                                       {
                                           return current_.axis_just_passed_threshold(threshold.gamepad_index,
                                                                                      threshold.axis,
                                                                                      threshold.threshold,
                                                                                      previous_);
                                       },
                                       [this](const AnyGamepadAxisThreshold &threshold)
                                       {
                                           return current_.axis_just_passed_threshold_on_any(threshold.axis,
                                                                                             threshold.threshold,
                                                                                             previous_);
                                       }},
                              input);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool was_any_pressed(Range &&range) const
        {
            return std::ranges::any_of(std::forward<Range>(range),
                                       [this](const DigitalInput button) { return was_pressed(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool were_all_pressed(Range &&range) const
        {
            return std::ranges::all_of(std::forward<Range>(range),
                                       [this](const DigitalInput button) { return was_pressed(button); });
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, DigitalInput>
        [[nodiscard]] constexpr bool were_none_pressed(Range &&range) const
        {
            return std::ranges::none_of(std::forward<Range>(range),
                                        [this](const DigitalInput button) { return was_pressed(button); });
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

        [[nodiscard]] constexpr float get_analogue_value(const AnalogueInput input) const
        {
            return std::visit(
                Overload{
                    [this](const LogicalKeyAnalogueInput &i) { return is_down(i.logical_key) ? i.axis_value : 0.0f; },
                    [this](const PhysicalKeyAnalogueInput &i) { return is_down(i.physical_key) ? i.axis_value : 0.0f; },
                    [this](const MouseButtonAnalogueInput &i) { return is_down(i.mouse_button) ? i.axis_value : 0.0f; },
                    [this](const GamepadButtonAnalogueInput &i) {
                        return is_down(GamepadButtonInput{i.gamepad_index, i.gamepad_button}) ? i.axis_value : 0.0f;
                    },
                    [this](const AnyGamepadButtonAnalogueInput &i)
                    { return is_down(AnyGamepadButtonInput{i.gamepad_button}) ? i.axis_value : 0.0f; },
                    [this](const GamepadAxisInput &i)
                    { return current_.axis(i.gamepad_index, i.axis) * (i.invert ? -1.0f : 1.0f); },
                    [this](const AnyGamepadAxisInput &i)
                    {
                        return current_.axis_on_any(i.axis) * (i.invert ? -1.0f : 1.0f);
                    }},
                input);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, AnalogueInput>
        [[nodiscard]] constexpr float get_analogue_values(Range &&range) const
        {
            return std::ranges::fold_left(std::forward<Range>(range),
                                          0.0f,
                                          [this](const float acc, const AnalogueInput input)
                                          { return acc + get_analogue_value(input); });
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
