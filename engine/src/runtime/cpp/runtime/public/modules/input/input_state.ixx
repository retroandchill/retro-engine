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

namespace retro
{
    export struct InputSnapshot
    {
        using LogicalKeyStorage = std::bitset<logical_key_max>;
        using PhysicalKeyStorage = std::bitset<physical_key_max>;
        using MouseButtonStorage = std::bitset<mouse_button_max>;

        std::uint64_t frame_index{};
        LogicalKeyStorage logical_keys{};
        PhysicalKeyStorage physical_keys{};
        MouseButtonStorage mouse_buttons{};
        Vector2f mouse_position{};
        Vector2f mouse_delta{};
        Vector2f mouse_wheel_delta{};

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

      private:
        InputSnapshot::LogicalKeyStorage logical_keys_{};
        InputSnapshot::PhysicalKeyStorage physical_keys_{};
        InputSnapshot::MouseButtonStorage mouse_buttons_{};
        Vector2f mouse_position_{};
        Vector2f mouse_delta_{};
        Vector2f mouse_wheel_delta_{};
    };
} // namespace retro
