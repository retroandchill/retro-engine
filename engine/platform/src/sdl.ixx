/**
 * @file sdl.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <SDL3/SDL.h>

export module retro.platform:sdl;

import std;
import retro.core;

namespace retro
{
    export class PlatformException : public std::exception
    {
    };

    export class RETRO_API PlatformContextStartFailed final : public PlatformException
    {
        [[nodiscard]] const char *what() const noexcept override;
    };

    export enum class PlatformInitFlags : uint32
    {
        Audio = SDL_INIT_AUDIO,
        Video = SDL_INIT_VIDEO,
        Joystick = SDL_INIT_JOYSTICK,
        Haptic = SDL_INIT_HAPTIC,
        Gamepad = SDL_INIT_GAMEPAD,
        Events = SDL_INIT_EVENTS,
        Sensor = SDL_INIT_SENSOR,
        Camera = SDL_INIT_CAMERA,
    };

    export class PlatformContext
    {
      public:
        explicit inline PlatformContext(PlatformInitFlags flags)
        {
            if (!SDL_Init(static_cast<Uint32>(flags)))
            {
                throw PlatformContextStartFailed{};
            }
        }

        PlatformContext(const PlatformContext &) = delete;
        PlatformContext(PlatformContext &&) = delete;

        inline ~PlatformContext() noexcept
        {
            SDL_Quit();
        }

        PlatformContext &operator=(const PlatformContext &) = delete;
        PlatformContext &operator=(PlatformContext &&) = delete;
    };
} // namespace retro
