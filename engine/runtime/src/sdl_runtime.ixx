/**
 * @file sdl_runtime.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:sdl_runtime;

import std;
import sdl;

namespace retro
{
    export class SdlRuntime
    {
      public:
        inline SdlRuntime()
        {
            if (!sdl::Init(sdl::InitFlags::VIDEO))
            {
                throw std::runtime_error{std::string{"SDL_Init failed: "} + sdl::GetError()};
            }
        }

        SdlRuntime(const SdlRuntime &) = delete;
        SdlRuntime(SdlRuntime &&) = delete;

        inline ~SdlRuntime() noexcept
        {
            sdl::Quit();
        }

        SdlRuntime &operator=(const SdlRuntime &) = delete;
        SdlRuntime &operator=(SdlRuntime &&) = delete;
    };
} // namespace retro
