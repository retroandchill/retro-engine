//
// Created by fcors on 12/19/2025.
//
module;
#include <SDL3/SDL.h>

#include "exports.h"

export module retro.platform;

import std;

namespace retro::platform {

    export class RETRO_API  Platform {
    public:
        Platform()
        {
            if (!SDL_Init(SDL_INIT_VIDEO)) {
                throw std::runtime_error{
                    std::string{"SDL_Init failed: "} + SDL_GetError()
                };
            }
        }

        ~Platform() noexcept {
            SDL_Quit();
        }

        Platform(const Platform&) = delete;
        Platform(Platform&&) noexcept = default;

        Platform& operator=(const Platform&) = delete;
        Platform& operator=(Platform&&) noexcept = default;
    };

}