/**
 * @file sdl.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

#include <SDL3/SDL.h>

export module retro.platform.backend:sdl;

import std;
import retro.core.async.future;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.math.vector;
import retro.core.strings.cstring_view;
import retro.platform.backend;
import retro.platform.exceptions;
import retro.platform.window;
import retro.platform.event;

namespace retro
{
    namespace
    {
        constexpr Uint32 to_sdl_flags(PlatformInitFlags flags) noexcept
        {
            Uint32 out = 0;

            const auto f = static_cast<std::uint32_t>(flags);

            if (f & static_cast<std::uint32_t>(PlatformInitFlags::audio))
                out |= SDL_INIT_AUDIO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::video))
                out |= SDL_INIT_VIDEO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::joystick))
                out |= SDL_INIT_JOYSTICK;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::haptic))
                out |= SDL_INIT_HAPTIC;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::gamepad))
                out |= SDL_INIT_GAMEPAD;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::events))
                out |= SDL_INIT_EVENTS;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::sensor))
                out |= SDL_INIT_SENSOR;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::camera))
                out |= SDL_INIT_CAMERA;

            return out;
        }

        constexpr SDL_WindowFlags to_sdl_window_flags(WindowFlags flags) noexcept
        {
            SDL_WindowFlags out = 0;

            const auto f = static_cast<std::uint64_t>(flags);

            if (f & static_cast<std::uint64_t>(WindowFlags::resizable))
                out |= SDL_WINDOW_RESIZABLE;
            if (f & static_cast<std::uint64_t>(WindowFlags::borderless))
                out |= SDL_WINDOW_BORDERLESS;
            if (f & static_cast<std::uint64_t>(WindowFlags::hidden))
                out |= SDL_WINDOW_HIDDEN;
            if (f & static_cast<std::uint64_t>(WindowFlags::vulkan))
                out |= SDL_WINDOW_VULKAN;
            if (f & static_cast<std::uint64_t>(WindowFlags::high_dpi))
                out |= SDL_WINDOW_HIGH_PIXEL_DENSITY;
            if (f & static_cast<std::uint64_t>(WindowFlags::always_on_top))
                out |= SDL_WINDOW_ALWAYS_ON_TOP;

            return out;
        }

        constexpr MouseButton to_mouse_button(const Uint8 sdl_button) noexcept
        {
            switch (sdl_button)
            {
                case SDL_BUTTON_LEFT:
                    return MouseButton::left;
                case SDL_BUTTON_MIDDLE:
                    return MouseButton::middle;
                case SDL_BUTTON_RIGHT:
                    return MouseButton::right;
                case SDL_BUTTON_X1:
                    return MouseButton::x1;
                case SDL_BUTTON_X2:
                    return MouseButton::x2;
                default:
                    return MouseButton::unknown;
            }
        }
    } // namespace

    struct SdlWindowDeleter
    {
        inline void operator()(SDL_Window *window) const noexcept
        {
            SDL_RunOnMainThread([](void *data) { SDL_DestroyWindow(static_cast<SDL_Window *>(data)); }, window, false);
        }
    };

    using SdlWindowPtr = std::unique_ptr<SDL_Window, SdlWindowDeleter>;

    class Sdl3Window final : public Window
    {
      public:
        explicit Sdl3Window(const WindowDesc &desc)
            : window_{SDL_CreateWindow(desc.title.data(), desc.width, desc.height, to_sdl_window_flags(desc.flags))}
        {
            if (window_ == nullptr)
            {
                throw PlatformException{SDL_GetError()};
            }
        }

        [[nodiscard]] std::uint64_t id() const noexcept override
        {
            return SDL_GetWindowID(window_.get());
        }

        [[nodiscard]] NativeWindowHandle native_handle() const noexcept override
        {
            return {.backend = WindowBackend::sdl3, .handle = window_.get()};
        }

        void set_title(const CStringView title) override
        {
            SDL_SetWindowTitle(window_.get(), title.data());
        }

        [[nodiscard]] Vector2u size() const override
        {
            int w = 0;
            int h = 0;
            SDL_GetWindowSize(window_.get(), &w, &h);
            return {static_cast<std::uint32_t>(w), static_cast<std::uint32_t>(h)};
        }

      private:
        SdlWindowPtr window_;
    };

    class Sdl3PlatformBackend final : public PlatformBackend
    {
      public:
        explicit Sdl3PlatformBackend(const PlatformInitFlags flags)
        {
            if (const Uint32 sdl_flags = to_sdl_flags(flags); !SDL_Init(sdl_flags))
            {
                throw PlatformException(SDL_GetError());
            }

            if (has_any_flags(flags, PlatformInitFlags::video | PlatformInitFlags::events))
            {
                events_processing_enabled_ = true;
                const std::uint32_t first_event_flag = SDL_RegisterEvents(1);
                custom_event_types_.callback_event = first_event_flag;
            }
        }

        Sdl3PlatformBackend(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend(Sdl3PlatformBackend &&) noexcept = delete;

        ~Sdl3PlatformBackend() noexcept override
        {
            SDL_Quit();
        }

        Sdl3PlatformBackend &operator=(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend &operator=(Sdl3PlatformBackend &&) noexcept = delete;

        PlatformResult<std::shared_ptr<Window>> create_window(const WindowDesc &desc) override
        {
            if (SDL_IsMainThread())
            {
                return std::make_shared<Sdl3Window>(desc);
            }

            Promise<std::shared_ptr<Window>> promise;
            EXPECT(push_event(CallbackEvent{.callback = [&promise, &desc]
                                            {
                                                promise.emplace(std::make_shared<Sdl3Window>(desc));
                                            }}));

            return promise.get_future().get();
        }

        Task<PlatformResult<std::shared_ptr<Window>>> create_window_async(WindowDesc desc) override
        {
            if (SDL_IsMainThread())
            {
                co_return std::make_shared<Sdl3Window>(desc);
            }

            auto promise = std::make_shared<Promise<std::shared_ptr<Window>>>();
            CO_EXPECT(push_event(CallbackEvent{.callback = [promise, desc = std::move(desc)]
                                               {
                                                   promise->emplace(std::make_shared<Sdl3Window>(desc));
                                               }}));

            co_return co_await promise->get_future();
        }

        Optional<Event> poll_event() override
        {
            SDL_Event e{};
            if (!SDL_PollEvent(&e))
            {
                return std::nullopt;
            }

            return to_event(e);
        }

        Optional<Event> wait_for_event() override
        {
            SDL_Event e{};
            if (!SDL_WaitEvent(&e))
            {
                return std::nullopt;
            }

            return to_event(e);
        }

        Optional<Event> wait_for_event(const std::chrono::milliseconds timeout) override
        {
            SDL_Event e{};
            if (!SDL_WaitEventTimeout(&e, static_cast<std::int16_t>(timeout.count())))
            {
                return std::nullopt;
            }

            return to_event(e);
        }

        PlatformResult<void> push_event(Event event) override
        {
            auto e = from_event(std::move(event));
            if (!SDL_PushEvent(&e))
            {
                return std::unexpected{PlatformError{.message = SDL_GetError()}};
            }

            return {};
        }

      private:
        SDL_Event from_event(Event &&event)
        {
            return std::visit(Overload{[](const QuitEvent &)
                                       {
                                           return SDL_Event{
                                               .quit =
                                                   {
                                                       .type = SDL_EVENT_QUIT,
                                                   },
                                           };
                                       },
                                       [](const WindowCloseRequestedEvent &close_event)
                                       {
                                           return SDL_Event{
                                               .window =
                                                   {
                                                       .type = SDL_EVENT_WINDOW_CLOSE_REQUESTED,
                                                       .windowID = close_event.window_id,
                                                   },
                                           };
                                       },
                                       [](const WindowResizedEvent &resize_event)
                                       {
                                           return SDL_Event{
                                               .window =
                                                   {
                                                       .type = SDL_EVENT_WINDOW_RESIZED,
                                                       .windowID = resize_event.window_id,
                                                       .data1 = resize_event.width,
                                                       .data2 = resize_event.height,
                                                   },
                                           };
                                       },
                                       [](const MouseMovedEvent &move_event)
                                       {
                                           return SDL_Event{.motion = {
                                                                .type = SDL_EVENT_MOUSE_MOTION,
                                                                .windowID = move_event.window_id,
                                                                .x = move_event.x,
                                                                .y = move_event.y,
                                                                .xrel = move_event.dx,
                                                                .yrel = move_event.dy,
                                                            }};
                                       },
                                       [](const MouseButtonEvent &button_event)
                                       {
                                           return SDL_Event{.button = {
                                                                .type = button_event.down ? SDL_EVENT_MOUSE_BUTTON_DOWN
                                                                                          : SDL_EVENT_MOUSE_BUTTON_UP,
                                                                .windowID = button_event.window_id,
                                                                .button = static_cast<Uint8>(button_event.button),
                                                                .x = button_event.x,
                                                                .y = button_event.y,
                                                            }};
                                       },
                                       [](const KeyEvent &key_event)
                                       {
                                           return SDL_Event{
                                               .key = {
                                                   .type = key_event.down ? SDL_EVENT_KEY_DOWN : SDL_EVENT_KEY_UP,
                                                   .windowID = key_event.window_id,
                                                   .scancode = static_cast<SDL_Scancode>(key_event.scancode),
                                                   .key = static_cast<SDL_Keycode>(key_event.keycode),
                                                   .repeat = key_event.repeat,
                                               }};
                                       },
                                       [this](CallbackEvent &&callback_event)
                                       {
                                           deferred_events_.push_back(std::move(callback_event.callback));
                                           return SDL_Event{.user = {
                                                                .type = custom_event_types_.callback_event,
                                                            }};
                                       }},
                              std::move(event));
        }

        Optional<Event> to_event(const SDL_Event &e) noexcept
        {
            switch (e.type)
            {
                case SDL_EVENT_QUIT:
                    return Event{QuitEvent{}};

                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                    return Event{WindowCloseRequestedEvent{
                        .window_id = e.window.windowID,
                    }};

                case SDL_EVENT_WINDOW_RESIZED:
                    return Event{WindowResizedEvent{
                        .window_id = e.window.windowID,
                        .width = e.window.data1,
                        .height = e.window.data2,
                    }};

                case SDL_EVENT_MOUSE_MOTION:
                    return Event{MouseMovedEvent{
                        .window_id = e.motion.windowID,
                        .x = e.motion.x,
                        .y = e.motion.y,
                        .dx = e.motion.xrel,
                        .dy = e.motion.yrel,
                    }};

                case SDL_EVENT_MOUSE_BUTTON_DOWN:
                case SDL_EVENT_MOUSE_BUTTON_UP:
                    return Event{MouseButtonEvent{
                        .window_id = e.button.windowID,
                        .button = to_mouse_button(e.button.button),
                        .down = (e.type == SDL_EVENT_MOUSE_BUTTON_DOWN),
                        .x = e.button.x,
                        .y = e.button.y,
                    }};

                case SDL_EVENT_KEY_DOWN:
                case SDL_EVENT_KEY_UP:
                    return Event{KeyEvent{
                        .window_id = e.key.windowID,
                        .keycode = static_cast<std::int32_t>(e.key.key),
                        .scancode = static_cast<std::int32_t>(e.key.scancode),
                        .down = (e.type == SDL_EVENT_KEY_DOWN),
                        .repeat = e.key.repeat,
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

      private:
        struct CustomEventTypes
        {
            std::uint32_t callback_event;
        };

        CustomEventTypes custom_event_types_{};
        bool events_processing_enabled_ = false;
        std::mutex event_queue_mutex_;
        std::deque<std::function<void()>> deferred_events_;
    };
} // namespace retro
