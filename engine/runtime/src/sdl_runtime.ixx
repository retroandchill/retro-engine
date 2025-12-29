//
// Created by fcors on 12/26/2025.
//
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