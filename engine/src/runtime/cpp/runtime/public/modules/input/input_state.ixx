/**
 * @file input_state.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.input.input_state;

import std;
import retro.core.math.vector;
import retro.platform.input;
import retro.core.containers.optional;

namespace retro
{
    export struct GamepadSnapshot
    {
        using ButtonStorage = std::bitset<gamepad_button_max>;
        using AxisStorage = std::array<float, gamepad_axis_max>;

        std::uint32_t platform_id = 0;
        ButtonStorage buttons{};
        AxisStorage axes{};

        [[nodiscard]] constexpr bool is_down(GamepadButton button) const
        {
            return buttons.test(static_cast<std::size_t>(button));
        }

        [[nodiscard]] constexpr bool was_pressed(const GamepadButton button, const GamepadSnapshot &previous) const
        {
            return is_down(button) && !previous.is_down(button);
        }

        [[nodiscard]] constexpr float axis(const GamepadAxis axis) const
        {
            return axes[static_cast<std::size_t>(axis)];
        }
    };

    export constexpr inline std::size_t max_gamepads = 8;

    export struct InputSnapshot
    {
        using LogicalKeyStorage = std::bitset<logical_key_max>;
        using PhysicalKeyStorage = std::bitset<physical_key_max>;
        using MouseButtonStorage = std::bitset<mouse_button_max>;
        using GamepadStorage = std::array<Optional<GamepadSnapshot>, max_gamepads>;

        std::uint64_t frame_index{};
        LogicalKeyStorage logical_keys{};
        PhysicalKeyStorage physical_keys{};
        MouseButtonStorage mouse_buttons{};
        Vector2f mouse_position{};
        Vector2f mouse_delta{};
        Vector2f mouse_wheel_delta{};
        GamepadStorage gamepads{};

        [[nodiscard]] constexpr bool is_down(LogicalKey key) const
        {
            return logical_keys.test(static_cast<std::size_t>(key));
        }

        [[nodiscard]] constexpr bool is_down(PhysicalKey key) const
        {
            return physical_keys.test(static_cast<std::size_t>(key));
        }

        [[nodiscard]] constexpr bool is_down(MouseButton button) const
        {
            return mouse_buttons.test(static_cast<std::size_t>(button));
        }

        [[nodiscard]] constexpr bool is_down(const std::uint8_t gamepad_id, const GamepadButton button) const
        {
            auto &gamepad = gamepads[gamepad_id];
            return gamepad.has_value() && gamepad->is_down(button);
        }

        [[nodiscard]] constexpr bool is_down_on_any(const GamepadButton button) const
        {
            return std::ranges::any_of(gamepads | std::views::join,
                                       [button](const GamepadSnapshot &snapshot) { return snapshot.is_down(button); });
        }

        [[nodiscard]] constexpr bool was_pressed(const LogicalKey key, const InputSnapshot &previous) const
        {
            return is_down(key) && !previous.is_down(key);
        }

        [[nodiscard]] constexpr bool was_pressed(const PhysicalKey key, const InputSnapshot &previous) const
        {
            return is_down(key) && !previous.is_down(key);
        }

        [[nodiscard]] constexpr bool was_pressed(const MouseButton button, const InputSnapshot &previous) const
        {
            return is_down(button) && !previous.is_down(button);
        }

        [[nodiscard]] constexpr bool was_pressed(const std::uint8_t gamepad_id,
                                                 const GamepadButton button,
                                                 const InputSnapshot &previous) const
        {
            return is_down(gamepad_id, button) && !previous.is_down(gamepad_id, button);
        }

        [[nodiscard]] constexpr bool was_pressed_on_any(const GamepadButton button, const InputSnapshot &previous) const
        {
            return is_down_on_any(button) && !previous.is_down_on_any(button);
        }

        [[nodiscard]] constexpr float axis(const std::uint8_t gamepad_id, const GamepadAxis axis) const
        {
            auto &gamepad = gamepads[gamepad_id];
            return gamepad.has_value() ? gamepad->axis(axis) : 0.0f;
        }

        [[nodiscard]] constexpr bool axis_over_threshold(const std::uint8_t gamepad_id,
                                                         const GamepadAxis axis,
                                                         const float threshold) const
        {
            return threshold > 0.0f ? this->axis(gamepad_id, axis) >= threshold
                                    : this->axis(gamepad_id, axis) <= threshold;
        }

        [[nodiscard]] constexpr bool axis_just_passed_threshold(const std::uint8_t gamepad_id,
                                                                const GamepadAxis axis,
                                                                const float threshold,
                                                                const InputSnapshot &previous) const
        {
            return axis_over_threshold(gamepad_id, axis, threshold) &&
                   !previous.axis_over_threshold(gamepad_id, axis, threshold);
        }

        [[nodiscard]] constexpr float axis_on_any(const GamepadAxis axis) const
        {
            return std::ranges::fold_left(gamepads | std::views::join |
                                              std::views::transform([axis](const auto &s) { return s.axis(axis); }),
                                          0.0f,
                                          std::plus<float>{});
        }

        [[nodiscard]] constexpr bool axis_over_threshold_on_any(const GamepadAxis axis, const float threshold) const
        {
            return threshold > 0.0f ? axis_on_any(axis) >= threshold : axis_on_any(axis) <= threshold;
        }

        [[nodiscard]] constexpr bool axis_just_passed_threshold_on_any(const GamepadAxis axis,
                                                                       const float threshold,
                                                                       const InputSnapshot &previous) const
        {
            return axis_over_threshold_on_any(axis, threshold) && !previous.axis_over_threshold_on_any(axis, threshold);
        }
    };

    export class RETRO_API InputState
    {
      public:
        [[nodiscard]] InputSnapshot snapshot(std::uint64_t frame_index) const;

        void begin_next_frame();

        void set_key_down(LogicalKey key, bool is_down);

        void set_key_down(PhysicalKey key, bool is_down);

        void set_mouse_button_down(MouseButton button, bool is_down);

        void set_mouse_position(float x, float y);

        void add_mouse_delta(float x, float y);

        void add_mouse_wheel_delta(float x, float y);

        void enable_gamepad(std::size_t index, std::uint32_t platform_id);

        void disable_gamepad(std::size_t index);

        void set_gamepad_button_down(std::size_t index, GamepadButton button, bool is_down);

        void set_gamepad_axis(std::size_t index, GamepadAxis axis, float value);

      private:
        InputSnapshot::LogicalKeyStorage logical_keys_{};
        InputSnapshot::PhysicalKeyStorage physical_keys_{};
        InputSnapshot::MouseButtonStorage mouse_buttons_{};
        Vector2f mouse_position_{};
        Vector2f mouse_delta_{};
        Vector2f mouse_wheel_delta_{};
        InputSnapshot::GamepadStorage gamepads_{};
    };
} // namespace retro
