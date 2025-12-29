//
// Created by fcors on 12/28/2025.
//
module;

#include <SDL3/SDL.h>

export module sdl;

import std;

namespace sdl
{
    export class SdlException : public std::runtime_error
    {
        using std::runtime_error::runtime_error;
    };

    inline void check_error(const bool condition)
    {
        if (!condition)
        {
            throw SdlException{SDL_GetError()};
        }
    }

    export using InitFlags = SDL_InitFlags;

    export constexpr InitFlags INIT_VIDEO = SDL_INIT_AUDIO;
    export constexpr InitFlags INIT_JOYSTICK = SDL_INIT_JOYSTICK;
    export constexpr InitFlags INIT_HAPTIC = SDL_INIT_HAPTIC;
    export constexpr InitFlags INIT_GAMEPAD = SDL_INIT_GAMEPAD;
    export constexpr InitFlags INIT_EVENTS = SDL_INIT_EVENTS;
    export constexpr InitFlags INIT_SENSOR = SDL_INIT_SENSOR;
    export constexpr InitFlags INIT_CAMERA = SDL_INIT_CAMERA;

    export class InitGuard
    {
    public:
        explicit inline InitGuard(const InitFlags flags)
        {
            check_error(SDL_Init(flags));
        }

        InitGuard(const InitGuard &) = delete;
        InitGuard(InitGuard &&) = delete;

        inline ~InitGuard() noexcept
        {
            SDL_Quit();
        }

        InitGuard &operator=(const InitGuard &) = delete;
        InitGuard &operator=(InitGuard &&) = delete;
    };
}