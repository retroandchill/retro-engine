/**
 * @file backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.platform.backend;

import std;
import retro.core.containers.optional;
import retro.platform.event;
import retro.platform.window;

namespace retro
{
    export enum class PlatformBackendKind : std::uint8_t
    {
        SDL3
    };

    export enum class PlatformInitFlags : std::uint32_t
    {
        None = 0,
        Audio = 1u << 0,
        Video = 1u << 1,
        Joystick = 1u << 2,
        Haptic = 1u << 3,
        Gamepad = 1u << 4,
        Events = 1u << 5,
        Sensor = 1u << 6,
        Camera = 1u << 7,
    };

    export constexpr PlatformInitFlags operator|(PlatformInitFlags a, PlatformInitFlags b) noexcept
    {
        return static_cast<PlatformInitFlags>(static_cast<std::uint32_t>(a) | static_cast<std::uint32_t>(b));
    }

    export constexpr bool any(PlatformInitFlags f) noexcept
    {
        return static_cast<std::uint32_t>(f) != 0;
    }

    export struct PlatformBackendInfo
    {
        PlatformBackendKind kind = PlatformBackendKind::SDL3;
        PlatformInitFlags flags = PlatformInitFlags::None;
    };

    export class PlatformBackend
    {
      public:
        RETRO_API static std::unique_ptr<PlatformBackend> create(const PlatformBackendInfo &info);

        virtual ~PlatformBackend() = default;

        virtual std::shared_ptr<Window> create_window(const WindowDesc &desc) = 0;

        virtual Optional<Event> poll_event() = 0;

        virtual Optional<Event> wait_for_event() = 0;

        virtual Optional<Event> wait_for_event(std::chrono::milliseconds timeout) = 0;
    };
} // namespace retro
