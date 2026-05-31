/**
 * @file input_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.input.input_manager;
import retro.core.functional.overload;

namespace retro
{

    void InputManager::push_event(const InputEvent &event)
    {
        std::scoped_lock lock{mutex_};
        event_write_queue_.push_back(event);
    }

    void InputManager::poll_events(std::uint64_t frame_count)
    {
        state_.begin_next_frame();

        {
            std::scoped_lock lock{mutex_};
            event_read_queue_.swap(event_write_queue_);

            for (auto &event : event_read_queue_)
            {
                apply_event(event);
            }

            event_read_queue_.clear();
        }

        previous_ = current_;
        current_ = state_.snapshot(frame_count);
    }

    void InputManager::apply_event(const InputEvent &event)
    {
        std::visit(Overload{[this](const MouseMovedEvent &evt) { state_.set_mouse_position(evt.x, evt.y); },
                            [this](const MouseButtonEvent &evt) { state_.set_mouse_button_down(evt.button, evt.down); },
                            [this](const MouseWheelEvent &evt) { state_.add_mouse_wheel_delta(evt.x, evt.y); },
                            [this](const KeyEvent &evt)
                            {
                                state_.set_key_down(evt.logical_key, evt.down);
                                state_.set_key_down(evt.physical_key, evt.down);
                            }},
                   event);
    }
} // namespace retro
