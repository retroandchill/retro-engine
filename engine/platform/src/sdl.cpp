/**
 * @file sdl.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform;

namespace retro
{
    const char *PlatformContextStartFailed::what() const noexcept
    {
        return PlatformException::what();
    }
} // namespace retro
