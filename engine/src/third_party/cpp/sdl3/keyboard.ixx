/**
 * @file keyboard.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL_keyboard.h>

export module sdl:keyboard;

namespace sdl
{
    export using KeyboardID = SDL_KeyboardID;
}
