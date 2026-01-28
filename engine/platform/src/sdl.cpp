/**
 * @file sdl.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL.h>

module retro.platform;

namespace retro
{
    namespace
    {
        constexpr Uint32 to_sdl_flags(PlatformInitFlags flags) noexcept
        {
            Uint32 out = 0;

            const auto f = static_cast<uint32>(flags);

            if (f & static_cast<uint32>(PlatformInitFlags::Audio))
                out |= SDL_INIT_AUDIO;
            if (f & static_cast<uint32>(PlatformInitFlags::Video))
                out |= SDL_INIT_VIDEO;
            if (f & static_cast<uint32>(PlatformInitFlags::Joystick))
                out |= SDL_INIT_JOYSTICK;
            if (f & static_cast<uint32>(PlatformInitFlags::Haptic))
                out |= SDL_INIT_HAPTIC;
            if (f & static_cast<uint32>(PlatformInitFlags::Gamepad))
                out |= SDL_INIT_GAMEPAD;
            if (f & static_cast<uint32>(PlatformInitFlags::Events))
                out |= SDL_INIT_EVENTS;
            if (f & static_cast<uint32>(PlatformInitFlags::Sensor))
                out |= SDL_INIT_SENSOR;
            if (f & static_cast<uint32>(PlatformInitFlags::Camera))
                out |= SDL_INIT_CAMERA;

            return out;
        }

        constexpr SDL_WindowFlags to_sdl_window_flags(WindowFlags flags) noexcept
        {
            SDL_WindowFlags out = 0;

            const auto f = static_cast<uint64>(flags);

            if (f & static_cast<uint64>(WindowFlags::Resizable))
                out |= SDL_WINDOW_RESIZABLE;
            if (f & static_cast<uint64>(WindowFlags::Borderless))
                out |= SDL_WINDOW_BORDERLESS;
            if (f & static_cast<uint64>(WindowFlags::Hidden))
                out |= SDL_WINDOW_HIDDEN;
            if (f & static_cast<uint64>(WindowFlags::Vulkan))
                out |= SDL_WINDOW_VULKAN;
            if (f & static_cast<uint64>(WindowFlags::HighDpi))
                out |= SDL_WINDOW_HIGH_PIXEL_DENSITY;
            if (f & static_cast<uint64>(WindowFlags::AlwaysOnTop))
                out |= SDL_WINDOW_ALWAYS_ON_TOP;

            return out;
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
            return {static_cast<uint32>(w), static_cast<uint32>(h)};
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
} // namespace retro
