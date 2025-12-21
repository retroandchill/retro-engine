//
// Created by fcors on 12/19/2025.
//
module;
#include <SDL3/SDL.h>

#include "retro/core/exports.h"

export module retro.platform.window;

import std;
import retro.core;
import retro.core.strings;
import retro.platform;

namespace retro::platform {
    using namespace core;

    struct WindowDeleter {
        inline void operator()(SDL_Window* window) const noexcept {
            SDL_DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<SDL_Window, WindowDeleter>;

    export class RETRO_API Window {
    public:
        Window(Platform&, const int32 width, const int32 height, const CStringView title) {
            window_ = WindowPtr{SDL_CreateWindow(title.data(), width, height, SDL_WINDOW_RESIZABLE)};

            if (window_ == nullptr) {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + SDL_GetError()};
            }
        }

        [[nodiscard]] SDL_Window* get_native_handle() const {
            return window_.get();
        }


    private:
        WindowPtr window_;
    };
}