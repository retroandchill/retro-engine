/**
 * @file sdl.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL.h>

module retro.platform;

import retro.platform.exceptions;

namespace retro
{
    namespace
    {
        constexpr Uint32 to_sdl_flags(PlatformInitFlags flags) noexcept
        {
            Uint32 out = 0;

            const auto f = static_cast<std::uint32_t>(flags);

            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Audio))
                out |= SDL_INIT_AUDIO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Video))
                out |= SDL_INIT_VIDEO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Joystick))
                out |= SDL_INIT_JOYSTICK;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Haptic))
                out |= SDL_INIT_HAPTIC;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Gamepad))
                out |= SDL_INIT_GAMEPAD;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Events))
                out |= SDL_INIT_EVENTS;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Sensor))
                out |= SDL_INIT_SENSOR;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::Camera))
                out |= SDL_INIT_CAMERA;

            return out;
        }

        constexpr SDL_WindowFlags to_sdl_window_flags(WindowFlags flags) noexcept
        {
            SDL_WindowFlags out = 0;

            const auto f = static_cast<std::uint64_t>(flags);

            if (f & static_cast<std::uint64_t>(WindowFlags::Resizable))
                out |= SDL_WINDOW_RESIZABLE;
            if (f & static_cast<std::uint64_t>(WindowFlags::Borderless))
                out |= SDL_WINDOW_BORDERLESS;
            if (f & static_cast<std::uint64_t>(WindowFlags::Hidden))
                out |= SDL_WINDOW_HIDDEN;
            if (f & static_cast<std::uint64_t>(WindowFlags::Vulkan))
                out |= SDL_WINDOW_VULKAN;
            if (f & static_cast<std::uint64_t>(WindowFlags::HighDpi))
                out |= SDL_WINDOW_HIGH_PIXEL_DENSITY;
            if (f & static_cast<std::uint64_t>(WindowFlags::AlwaysOnTop))
                out |= SDL_WINDOW_ALWAYS_ON_TOP;

            return out;
        }

        constexpr MouseButton to_mouse_button(const Uint8 sdl_button) noexcept
        {
            switch (sdl_button)
            {
                case SDL_BUTTON_LEFT:
                    return MouseButton::Left;
                case SDL_BUTTON_MIDDLE:
                    return MouseButton::Middle;
                case SDL_BUTTON_RIGHT:
                    return MouseButton::Right;
                case SDL_BUTTON_X1:
                    return MouseButton::X1;
                case SDL_BUTTON_X2:
                    return MouseButton::X2;
                default:
                    return MouseButton::Unknown;
            }
        }

        constexpr Optional<Event> to_event(const SDL_Event &e)
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
                    // For now: ignore events you don't care about.
                    // Later you can add more cases or a "Raw/UnknownEvent" if needed.
                    return std::nullopt;
            }
        }
    } // namespace

    PlatformContext::PlatformContext(const PlatformInitFlags flags) : flags_(flags)
    {
        if (const Uint32 sdl_flags = to_sdl_flags(flags_); !SDL_Init(sdl_flags))
        {
            // Use whatever your platform exception type is; keeping it generic here.
            throw PlatformException(SDL_GetError());
        }
    }

    PlatformContext::~PlatformContext() noexcept
    {
        SDL_Quit();
    }

    struct SdlWindowDeleter
    {
        inline void operator()(SDL_Window *window) const noexcept
        {
            SDL_DestroyWindow(window);
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

        [[nodiscard]] NativeWindowHandle native_handle() const noexcept override
        {
            return {.backend = WindowBackend::SDL3, .handle = window_.get()};
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

    std::unique_ptr<Window> create_sdl_window(const WindowDesc &desc)
    {
        return std::make_unique<Sdl3Window>(desc);
    }

    std::shared_ptr<Window> create_shared_sdl_window(const WindowDesc &desc)
    {
        return std::make_unique<Sdl3Window>(desc);
    }

    Optional<Event> poll_event()
    {
        SDL_Event e{};
        if (!SDL_PollEvent(&e))
        {
            return std::nullopt;
        }

        return to_event(e);
    }

    Optional<Event> wait_for_event()
    {
        SDL_Event e{};
        if (!SDL_WaitEvent(&e))
        {
            return std::nullopt;
        }

        return to_event(e);
    }

    Optional<Event> wait_for_event(const std::chrono::milliseconds timeout)
    {
        SDL_Event e{};
        if (!SDL_WaitEventTimeout(&e, timeout.count()))
        {
            return std::nullopt;
        }

        return to_event(e);
    }
} // namespace retro
