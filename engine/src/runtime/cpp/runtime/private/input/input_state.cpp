/**
 * @file input_state.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.input.input_state;

namespace retro
{

    InputSnapshot InputState::snapshot(const std::uint64_t frame_index) const
    {
        return InputSnapshot{
            .frame_index = frame_index,
            .logical_keys = logical_keys_,
            .physical_keys = physical_keys_,
            .mouse_buttons = mouse_buttons_,
            .mouse_position = mouse_position_,
            .mouse_delta = mouse_delta_,
            .mouse_wheel_delta = mouse_wheel_delta_,
        };
    }

    void InputState::begin_next_frame()
    {
        mouse_delta_ = {};
        mouse_wheel_delta_ = {};
    }

    void InputState::set_key_down(LogicalKey key, const bool is_down)
    {
        logical_keys_.set(static_cast<std::size_t>(key), is_down);
    }

    void InputState::set_key_down(PhysicalKey key, const bool is_down)
    {
        physical_keys_.set(static_cast<std::size_t>(key), is_down);
    }

    void InputState::set_mouse_button_down(MouseButton button, const bool is_down)
    {
        mouse_buttons_.set(static_cast<std::size_t>(button), is_down);
    }

    void InputState::set_mouse_position(const float x, const float y)
    {
        mouse_delta_.x += x - mouse_position_.x;
        mouse_delta_.y += y - mouse_position_.y;
        mouse_position_ = {x, y};
    }

    void InputState::add_mouse_delta(const float x, const float y)
    {
        mouse_wheel_delta_.x += x;
        mouse_wheel_delta_.y += y;
    }

    void InputState::add_mouse_wheel_delta(const float x, const float y)
    {
        mouse_wheel_delta_.x += x;
        mouse_wheel_delta_.y += y;
    }
} // namespace retro
