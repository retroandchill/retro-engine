/**
 * @file backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.backend;

import retro.platform.exceptions;
import :sdl;

namespace retro
{
    std::unique_ptr<PlatformBackend> PlatformBackend::create(const PlatformBackendInfo &info)
    {
        switch (info.kind)
        {
            case PlatformBackendKind::sdl3:
                return std::make_unique<Sdl3PlatformBackend>(info.flags);
            default:
                throw PlatformException{"Unsupported platform backend"};
        }
    }
} // namespace retro
