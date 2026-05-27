/**
 * @file sdl.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "Sdl3pp/SDL3pp.h"

export module sdl;

export namespace SDL
{
    using SDL::INIT_VIDEO;
    using SDL::InitFlags;
} // namespace SDL
