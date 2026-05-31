/**
 * @file window.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.sdl.window;

import retro.core.util.exceptions;

namespace retro
{
    namespace
    {
        constexpr SDL::WindowFlags to_sdl_window_flags(WindowFlags flags) noexcept
        {
            SDL::WindowFlags out = 0;

            const auto f = static_cast<std::uint64_t>(flags);

            if (f & static_cast<std::uint64_t>(WindowFlags::resizable))
                out |= SDL::WINDOW_RESIZABLE;
            if (f & static_cast<std::uint64_t>(WindowFlags::borderless))
                out |= SDL::WINDOW_BORDERLESS;
            if (f & static_cast<std::uint64_t>(WindowFlags::hidden))
                out |= SDL::WINDOW_HIDDEN;
            if (f & static_cast<std::uint64_t>(WindowFlags::vulkan))
                out |= SDL::WINDOW_VULKAN;
            if (f & static_cast<std::uint64_t>(WindowFlags::high_dpi))
                out |= SDL::WINDOW_HIGH_PIXEL_DENSITY;
            if (f & static_cast<std::uint64_t>(WindowFlags::always_on_top))
                out |= SDL::WINDOW_ALWAYS_ON_TOP;

            return out;
        }

        SDL::Window create_window_from_native(NativeWindowHandle handle)
        {
            auto properties = SDL::CreateProperties();
            switch (handle.type)
            {
                case NativeWindowType::win32_hwnd:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::WIN32_HWND_POINTER, handle.handle);
                    break;

                case NativeWindowType::x11_window:
                    SDL::SetNumberProperty(properties,
                                           SDL::prop::Window::Create::WIN32_HWND_POINTER,
                                           std::bit_cast<std::intptr_t>(handle.handle));
                    break;
                case NativeWindowType::wayland_surface:
                    SDL::SetPointerProperty(properties,
                                            SDL::prop::Window::Create::WAYLAND_WL_SURFACE_POINTER,
                                            handle.handle);
                    break;

                case NativeWindowType::cocoa_window:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::COCOA_WINDOW_POINTER, handle.handle);
                    break;
                case NativeWindowType::cocoa_view:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::COCOA_VIEW_POINTER, handle.handle);
                    break;

                case NativeWindowType::unknown:
                default:
                    throw PlatformException{"Unknown native window type"};
                    break;
            }

            return SDL::CreateWindowWithProperties(properties);
        }
    } // namespace

    Sdl3Window::Sdl3Window(const WindowDesc &desc)
        : Sdl3Window{SDL::CreateWindow(desc.title.data(), {desc.width, desc.height}, to_sdl_window_flags(desc.flags))}
    {
    }

    Sdl3Window::Sdl3Window(NativeWindowHandle handle) : Sdl3Window{create_window_from_native(handle)}
    {
    }

    Sdl3Window::Sdl3Window(SDL::Window window) : window_{std::move(window)}
    {
    }

    Sdl3Window::~Sdl3Window() noexcept // NOLINT(*-use-equals-default)
    {
        SDL::RunOnMainThread([w = window_.release()] { SDL::DestroyWindow(w); }, false);
    }

    std::uint64_t Sdl3Window::id() const noexcept
    {
        return window_.GetID();
    }

    PlatformWindowHandle Sdl3Window::platform_handle() const noexcept
    {
        return {.backend = WindowBackend::sdl3, .handle = window_.get()};
    }

    void Sdl3Window::set_title(const CStringView title)
    {
        window_.SetTitle(title.data());
    }

    Vector2u Sdl3Window::size() const
    {
        int w = 0;
        int h = 0;
        window_.GetSize(&w, &h);
        return {static_cast<std::uint32_t>(w), static_cast<std::uint32_t>(h)};
    }
} // namespace retro
