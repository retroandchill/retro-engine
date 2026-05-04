/**
 * @file window.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.platform.window;

import std;
import retro.core.strings.cstring_view;
import retro.core.math.vector;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export enum class WindowBackend : std::uint8_t
    {
        headless,
        sdl3
    };

    export struct PlatformWindowHandle
    {
        WindowBackend backend = WindowBackend::headless;
        void *handle = nullptr;
    };

    export enum class NativeWindowType : std::uint8_t
    {
        win32_hwnd,
        x11_window,
        wayland_surface,
        cocoa_window,
        cocoa_view,
        unknown = std::numeric_limits<std::uint8_t>::max()
    };

#ifdef _WIN32
    constexpr auto default_native_window_type = NativeWindowType::win32_hwnd;
#elifdef __linux__
    constexpr auto default_native_window_type = NativeWindowType::x11_window;
#elifdef __APPLE__
    constexpr auto default_native_window_type = NativeWindowType::cocoa_window;
#else
    constexpr auto default_native_window_type = NativeWindowType::unknown;
#endif

    export struct NativeWindowHandle
    {
        NativeWindowType type = default_native_window_type;
        void *handle = nullptr;
    };

    export enum class WindowFlags : std::uint64_t
    {
        none = 0,
        resizable = 1uLL << 0,
        borderless = 1uLL << 1,
        hidden = 1uLL << 2,
        vulkan = 1uLL << 3,
        high_dpi = 1uLL << 4,
        always_on_top = 1uLL << 5,
    };

    export constexpr WindowFlags operator|(WindowFlags a, WindowFlags b) noexcept
    {
        return static_cast<WindowFlags>(static_cast<std::uint64_t>(a) | static_cast<std::uint64_t>(b));
    }

    export constexpr WindowFlags operator&(WindowFlags a, WindowFlags b) noexcept
    {
        return static_cast<WindowFlags>(static_cast<std::uint64_t>(a) & static_cast<std::uint64_t>(b));
    }

    export constexpr bool any(WindowFlags f) noexcept
    {
        return static_cast<std::uint64_t>(f) != 0;
    }

    export struct WindowDesc
    {
        std::int32_t width = 1280;
        std::int32_t height = 720;
        std::string title{"Game"};
        WindowFlags flags = WindowFlags::resizable;
    };

    export class Window : public std::enable_shared_from_this<Window>
    {
      public:
        virtual ~Window() = default;

        [[nodiscard]] virtual std::uint64_t id() const noexcept = 0;

        [[nodiscard]] virtual PlatformWindowHandle platform_handle() const noexcept = 0;

        virtual void set_title(CStringView title) = 0;

        [[nodiscard]] virtual Vector2u size() const = 0;

        [[nodiscard]] inline std::uint32_t width() const noexcept
        {
            return size().x;
        }

        [[nodiscard]] inline std::uint32_t height() const noexcept
        {
            return size().y;
        }
    };
} // namespace retro
