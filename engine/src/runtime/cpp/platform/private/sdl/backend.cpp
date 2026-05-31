/**
 * @file backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.sdl.backend;

import retro.core.util.enum_class_flags;
import retro.core.functional.overload;
import retro.platform.input;
import retro.platform.sdl.input;

namespace retro
{
    static constexpr SDL::Uint32 to_sdl_flags(PlatformInitFlags flags) noexcept
    {
        SDL::Uint32 out = 0;

        const auto f = static_cast<std::uint32_t>(flags);

        if (f & static_cast<std::uint32_t>(PlatformInitFlags::audio))
            out |= SDL::INIT_AUDIO;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::video))
            out |= SDL::INIT_VIDEO;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::joystick))
            out |= SDL::INIT_JOYSTICK;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::haptic))
            out |= SDL::INIT_HAPTIC;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::gamepad))
            out |= SDL::INIT_GAMEPAD;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::events))
            out |= SDL::INIT_EVENTS;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::sensor))
            out |= SDL::INIT_SENSOR;
        if (f & static_cast<std::uint32_t>(PlatformInitFlags::camera))
            out |= SDL::INIT_CAMERA;

        return out;
    }

    Sdl3PlatformBackend::Sdl3PlatformBackend(const PlatformInitFlags flags)
    {
        const SDL::Uint32 sdl_flags = to_sdl_flags(flags);
        SDL::Init(sdl_flags);

        if (has_any_flags(flags, PlatformInitFlags::video | PlatformInitFlags::events))
        {
            events_processing_enabled_ = true;
            const std::uint32_t first_event_flag = SDL::RegisterEvents(1);
            custom_event_types_.callback_event = first_event_flag;
        }
    }

    Sdl3PlatformBackend::~Sdl3PlatformBackend() noexcept // NOLINT(*-use-equals-default)
    {
        SDL::Quit();
    }

    WindowBackend Sdl3PlatformBackend::window_backend() const noexcept
    {
        return WindowBackend::sdl3;
    }

    std::shared_ptr<Window> Sdl3PlatformBackend::create_window(const WindowDesc &desc)
    {
        return create_window_internal(desc);
    }

    Task<std::shared_ptr<Window>> Sdl3PlatformBackend::create_window_async(WindowDesc desc)
    {
        return create_window_internal_async(std::move(desc));
    }

    std::shared_ptr<Window> Sdl3PlatformBackend::create_window_from_native(NativeWindowHandle handle)
    {
        return create_window_internal(handle);
    }

    Task<std::shared_ptr<Window>> Sdl3PlatformBackend::create_window_from_native_async(NativeWindowHandle handle)
    {
        return create_window_internal_async(handle);
    }

    Optional<PlatformEvent> Sdl3PlatformBackend::poll_event()
    {
        SDL::Event e{};
        if (!SDL::PollEvent(&e))
        {
            return std::nullopt;
        }

        return to_event(e);
    }

    Optional<PlatformEvent> Sdl3PlatformBackend::wait_for_event()
    {
        return to_event(SDL::WaitEvent());
    }

    Optional<PlatformEvent> Sdl3PlatformBackend::wait_for_event(const std::chrono::milliseconds timeout)
    {
        SDL::Event e{};
        if (!SDL::WaitEventTimeout(&e, timeout))
        {
            return std::nullopt;
        }

        return to_event(e);
    }

    SDL::Event Sdl3PlatformBackend::from_event(PlatformEvent &&event)
    {
        return std::visit(
            Overload{
                [](const QuitEvent &)
                {
                    return SDL::Event{
                        .quit =
                            {
                                .type = SDL::EVENT_QUIT,
                            },
                    };
                },
                [](const WindowCloseRequestedEvent &close_event)
                {
                    return SDL::Event{
                        .window =
                            {
                                .type = SDL::EVENT_WINDOW_CLOSE_REQUESTED,
                                .windowID = close_event.window_id,
                            },
                    };
                },
                [](const WindowResizedEvent &resize_event)
                {
                    return SDL::Event{
                        .window =
                            {
                                .type = SDL::EVENT_WINDOW_RESIZED,
                                .windowID = resize_event.window_id,
                                .data1 = resize_event.width,
                                .data2 = resize_event.height,
                            },
                    };
                },
                [](const MouseMovedEvent &move_event)
                {
                    return SDL::Event{.motion = {
                                          .type = SDL::EVENT_MOUSE_MOTION,
                                          .windowID = move_event.window_id,
                                          .x = move_event.x,
                                          .y = move_event.y,
                                          .xrel = move_event.dx,
                                          .yrel = move_event.dy,
                                      }};
                },
                [](const MouseButtonEvent &button_event)
                {
                    return SDL::Event{
                        .button = {
                            .type = button_event.down ? SDL::EVENT_MOUSE_BUTTON_DOWN : SDL::EVENT_MOUSE_BUTTON_UP,
                            .windowID = button_event.window_id,
                            .button = to_sdl_mouse_button(button_event.button),
                            .x = button_event.x,
                            .y = button_event.y,
                        }};
                },
                [](const MouseWheelEvent &scroll_event)
                {
                    return SDL::Event{.wheel = {
                                          .type = SDL::EVENT_MOUSE_WHEEL,
                                          .windowID = scroll_event.window_id,
                                          .x = scroll_event.x,
                                          .y = scroll_event.y,
                                          .direction = scroll_event.direction == MouseWheelDirection::flipped
                                                           ? SDL::MOUSEWHEEL_FLIPPED
                                                           : SDL::MOUSEWHEEL_NORMAL,
                                          .mouse_x = scroll_event.mouse_x,
                                          .mouse_y = scroll_event.mouse_y,
                                          .integer_x = scroll_event.mouse_ticks_x,
                                          .integer_y = scroll_event.mouse_ticks_y,
                                      }};
                },
                [](const KeyEvent &key_event)
                {
                    return SDL::Event{.key = {
                                          .type = key_event.down ? SDL::EVENT_KEY_DOWN : SDL::EVENT_KEY_UP,
                                          .windowID = key_event.window_id,
                                          .scancode = to_sdl_scancode(key_event.physical_key),
                                          .key = to_sdl_keycode(key_event.logical_key),
                                          .repeat = key_event.repeat,
                                      }};
                },
                [](const GamepadDeviceEvent &gamepad_connected_event)
                {
                    return SDL::Event{.gdevice = {
                                          .type = gamepad_connected_event.change_type == GamepadChangeType::connected
                                                      ? SDL::EVENT_GAMEPAD_ADDED
                                                      : SDL::EVENT_GAMEPAD_REMOVED,
                                          .which = gamepad_connected_event.gamepad_id,
                                      }};
                },
                [](const GamepadButtonEvent &gamepad_button_event)
                {
                    return SDL::Event{.gbutton = {.type = gamepad_button_event.down ? SDL::EVENT_GAMEPAD_BUTTON_DOWN
                                                                                    : SDL::EVENT_GAMEPAD_BUTTON_UP,
                                                  .which = gamepad_button_event.gamepad_id,
                                                  .button = static_cast<SDL::Uint8>(
                                                      to_sdl_gamepad_button(gamepad_button_event.button))}};
                },
                [](const GamepadAxisEvent &gamepad_axis_event)
                {
                    return SDL::Event{.gaxis = {
                                          .type = SDL::EVENT_GAMEPAD_AXIS_MOTION,
                                          .which = gamepad_axis_event.gamepad_id,
                                          .axis = static_cast<SDL::Uint8>(to_sdl_gamepad_axis(gamepad_axis_event.axis)),
                                          .value = denormalize_gamepad_axis(gamepad_axis_event.value),
                                      }};
                },
                [this](CallbackEvent &&callback_event)
                {
                    deferred_events_.push_back(std::move(callback_event.callback));
                    return SDL::Event{.user = {
                                          .type = custom_event_types_.callback_event,
                                      }};
                }},
            std::move(event));
    }

    Optional<PlatformEvent> Sdl3PlatformBackend::to_event(const SDL::Event &e) noexcept
    {
        switch (e.type)
        {
            case SDL::EVENT_QUIT:
                return PlatformEvent{QuitEvent{}};

            case SDL::EVENT_WINDOW_CLOSE_REQUESTED:
                return PlatformEvent{WindowCloseRequestedEvent{
                    .window_id = e.window.windowID,
                }};

            case SDL::EVENT_WINDOW_RESIZED:
                return PlatformEvent{WindowResizedEvent{
                    .window_id = e.window.windowID,
                    .width = e.window.data1,
                    .height = e.window.data2,
                }};

            case SDL::EVENT_MOUSE_MOTION:
                return PlatformEvent{MouseMovedEvent{
                    .window_id = e.motion.windowID,
                    .x = e.motion.x,
                    .y = e.motion.y,
                    .dx = e.motion.xrel,
                    .dy = e.motion.yrel,
                }};

            case SDL::EVENT_MOUSE_BUTTON_DOWN:
            case SDL::EVENT_MOUSE_BUTTON_UP:
                return PlatformEvent{MouseButtonEvent{
                    .window_id = e.button.windowID,
                    .button = to_engine_mouse_button(e.button.button),
                    .down = (e.type == SDL::EVENT_MOUSE_BUTTON_DOWN),
                    .x = e.button.x,
                    .y = e.button.y,
                }};

            case SDL::EVENT_MOUSE_WHEEL:
                return PlatformEvent{MouseWheelEvent{
                    .window_id = e.wheel.windowID,
                    .x = e.wheel.x,
                    .y = e.wheel.y,
                    .direction = e.wheel.direction == SDL::MOUSEWHEEL_FLIPPED ? MouseWheelDirection::flipped
                                                                              : MouseWheelDirection::normal,
                    .mouse_x = e.wheel.mouse_x,
                    .mouse_y = e.wheel.mouse_y,
                    .mouse_ticks_x = e.wheel.integer_x,
                    .mouse_ticks_y = e.wheel.integer_y,
                }};

            case SDL::EVENT_KEY_DOWN:
            case SDL::EVENT_KEY_UP:
                return PlatformEvent{KeyEvent{
                    .window_id = e.key.windowID,
                    .logical_key = to_logical_key(e.key.key),
                    .physical_key = to_physical_key(e.key.scancode),
                    .down = (e.type == SDL::EVENT_KEY_DOWN),
                    .repeat = e.key.repeat,
                }};

            case SDL::EVENT_GAMEPAD_ADDED:
            case SDL::EVENT_GAMEPAD_REMOVED:
                return PlatformEvent{GamepadDeviceEvent{
                    .change_type = e.gdevice.type == SDL::EVENT_GAMEPAD_ADDED ? GamepadChangeType::connected
                                                                              : GamepadChangeType::disconnected,
                    .gamepad_id = e.gdevice.which,
                }};

            case SDL::EVENT_GAMEPAD_BUTTON_DOWN:
            case SDL::EVENT_GAMEPAD_BUTTON_UP:
                return PlatformEvent{GamepadButtonEvent{
                    .gamepad_id = e.gbutton.which,
                    .button = to_engine_gamepad_button(static_cast<SDL::GamepadButton>(e.gbutton.button)),
                    .down = e.gbutton.type == SDL::EVENT_GAMEPAD_BUTTON_DOWN,
                }};

            case SDL::EVENT_GAMEPAD_AXIS_MOTION:
                return PlatformEvent{GamepadAxisEvent{
                    .gamepad_id = e.gaxis.which,
                    .axis = to_engine_gamepad_axis(static_cast<SDL::GamepadAxis>(e.gaxis.axis)),
                    .value = normalize_gamepad_axis(e.gaxis.value),
                }};

            default:
                if (!events_processing_enabled_)
                    return std::nullopt;
                if (e.type == custom_event_types_.callback_event)
                {
                    auto callback = std::move(deferred_events_.front());
                    deferred_events_.pop_front();
                    return CallbackEvent{std::move(callback)};
                }
                return std::nullopt;
        }
    }
} // namespace retro
