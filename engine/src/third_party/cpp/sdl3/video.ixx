/**
 * @file video.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL_video.h>
#include <type_traits>

export module sdl:video;

namespace sdl
{
    export using DisplayID = SDL_DisplayID;
    export using WindowID = SDL_WindowID;

    export enum class SystemTheme : std::underlying_type_t<SDL_SystemTheme>
    {
        unknown = SDL_SYSTEM_THEME_UNKNOWN,
        light = SDL_SYSTEM_THEME_LIGHT,
        dark = SDL_SYSTEM_THEME_DARK
    };
} // namespace sdl
