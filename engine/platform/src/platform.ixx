//
// Created by fcors on 12/19/2025.
//
module;
#include <SDL3/SDL.h>

#include "retro/core/exports.h"

export module retro.platform:platform;

import std;

namespace retro {

    export class RETRO_API Platform {
    public:
        inline Platform()
        {
            if (!SDL_Init(SDL_INIT_VIDEO)) {
                throw std::runtime_error{
                    std::string{"SDL_Init failed: "} + SDL_GetError()
                };
            }
        }

        inline ~Platform() noexcept {
            SDL_Quit();
        }

        Platform(const Platform&) = delete;
        Platform(Platform&&) noexcept = delete;

        Platform& operator=(const Platform&) = delete;
        Platform& operator=(Platform&&) noexcept = delete;

        // NOLINTNEXTLINE
        bool poll_events();
    };

}