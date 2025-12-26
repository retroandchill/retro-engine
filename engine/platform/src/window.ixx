//
// Created by fcors on 12/19/2025.
//
module;
#include <SDL3/SDL.h>

export module retro.platform.window;

import std;
import retro.core;
import retro.platform;

namespace retro {
    struct WindowDeleter {
        inline void operator()(SDL_Window* window) const noexcept {
            SDL_DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<SDL_Window, WindowDeleter>;

    export class Window {
    public:
        inline Window(Platform&, const int32 width, const int32 height, const CStringView title) {
            window_ = WindowPtr{SDL_CreateWindow(title.data(), width, height, SDL_WINDOW_RESIZABLE)};

            if (window_ == nullptr) {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + SDL_GetError()};
            }
        }

        [[nodiscard]] inline SDL_Window* native_handle() const {
            return window_.get();
        }

        // NOLINTNEXTLINE
        inline void set_title(const CStringView title) {
            SDL_SetWindowTitle(window_.get(), title.data());
        }


    private:
        WindowPtr window_;
    };
}