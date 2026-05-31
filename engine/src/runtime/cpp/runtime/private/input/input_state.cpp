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
            .gamepads = gamepads_,
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

    void InputState::enable_gamepad(const std::size_t index, const std::uint32_t platform_id)
    {
        gamepads_[index].emplace(platform_id);
    }

    void InputState::disable_gamepad(const std::size_t index)
    {
        gamepads_[index].reset();
    }

    void InputState::set_gamepad_button_down(const std::size_t index, GamepadButton button, const bool is_down)
    {
        gamepads_[index]->buttons.set(static_cast<std::size_t>(button), is_down);
    }

    void InputState::set_gamepad_axis(const std::size_t index, GamepadAxis axis, const float value)
    {
        gamepads_[index]->axes[static_cast<std::size_t>(axis)] = value;
    }
} // namespace retro
