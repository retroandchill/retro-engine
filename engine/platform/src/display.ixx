/**
 * @file display.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.platform:display;

import std;
import retro.core.strings.cstring_view;
import retro.core.math.vector;

namespace retro
{
    export enum class WindowBackend
    {
        SDL3
    };

    export struct NativeWindowHandle
    {
        WindowBackend backend;
        void *handle;
    };

    export enum class WindowFlags : std::uint64_t
    {
        None = 0,
        Resizable = 1uLL << 0,
        Borderless = 1uLL << 1,
        Hidden = 1uLL << 2,
        Vulkan = 1uLL << 3,
        HighDpi = 1uLL << 4,
        AlwaysOnTop = 1uLL << 5,
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
        CStringView title{"Game"};
        WindowFlags flags = WindowFlags::Resizable;
        WindowBackend backend = WindowBackend::SDL3;
    };

    export class Window
    {
      public:
        virtual ~Window() = default;

        static RETRO_API std::unique_ptr<Window> create(const WindowDesc &desc);

        static RETRO_API std::shared_ptr<Window> create_shared(const WindowDesc &desc);

        [[nodiscard]] virtual NativeWindowHandle native_handle() const noexcept = 0;

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
