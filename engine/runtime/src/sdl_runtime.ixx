//
// Created by fcors on 12/26/2025.
//
module;

#include <SDL3/SDL.h>

export module retro.runtime:sdl_runtime;

import std;

namespace retro
{
    export class SdlRuntime
    {
      public:
        inline SdlRuntime()
        {
            if (!SDL_Init(SDL_INIT_VIDEO))
            {
                throw std::runtime_error{std::string{"SDL_Init failed: "} + SDL_GetError()};
            }
        }

        SdlRuntime(const SdlRuntime &) = delete;
        SdlRuntime(SdlRuntime &&) = delete;

        inline ~SdlRuntime() noexcept
        {
            SDL_Quit();
        }

        SdlRuntime &operator=(const SdlRuntime &) = delete;
        SdlRuntime &operator=(SdlRuntime &&) = delete;
    };
} // namespace retro