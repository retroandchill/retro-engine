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
import retro.core;

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

    export enum class WindowFlags : uint64
    {
        None = 0,
        Resizable = 1ull << 0,
        Borderless = 1ull << 1,
        Hidden = 1ull << 2,
        Vulkan = 1ull << 3,
        HighDpi = 1ull << 4,
        AlwaysOnTop = 1ull << 5,
    };

    export constexpr WindowFlags operator|(WindowFlags a, WindowFlags b) noexcept
    {
        return static_cast<WindowFlags>(static_cast<uint64>(a) | static_cast<uint64>(b));
    }

    export constexpr WindowFlags operator&(WindowFlags a, WindowFlags b) noexcept
    {
        return static_cast<WindowFlags>(static_cast<uint64>(a) & static_cast<uint64>(b));
    }

    export constexpr bool any(WindowFlags f) noexcept
    {
        return static_cast<uint64>(f) != 0;
    }

    export struct WindowDesc
    {
        int32 width = 1280;
        int32 height = 720;
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

        [[nodiscard]] inline uint32 width() const noexcept
        {
            return size().x;
        }

        [[nodiscard]] inline uint32 height() const noexcept
        {
            return size().y;
        }
    };
} // namespace retro
