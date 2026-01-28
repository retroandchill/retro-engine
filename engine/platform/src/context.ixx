/**
 * @file context.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.platform:context;

import std;
import retro.core;
import :display;
import :exceptions;

namespace retro
{
    export enum class PlatformInitFlags : uint32
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
        return static_cast<PlatformInitFlags>(static_cast<uint32>(a) | static_cast<uint32>(b));
    }

    export constexpr bool any(PlatformInitFlags f) noexcept
    {
        return static_cast<uint32>(f) != 0;
    }

    export class RETRO_API PlatformContext
    {
      public:
        explicit PlatformContext(PlatformInitFlags flags);

        PlatformContext(const PlatformContext &) = delete;
        PlatformContext(PlatformContext &&) = delete;

        ~PlatformContext() noexcept;

        PlatformContext &operator=(const PlatformContext &) = delete;
        PlatformContext &operator=(PlatformContext &&) = delete;

      private:
        PlatformInitFlags flags_{PlatformInitFlags::None};
    };
} // namespace retro
