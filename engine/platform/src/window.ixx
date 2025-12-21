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

        void poll_events() {
            SDL_Event event;
            while (SDL_PollEvent(&event)) {
                switch (event.type) {
                    case SDL_EVENT_QUIT:
                        should_close_ = true;
                        break;
                    case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                        if (window_ != nullptr && event.window.windowID == SDL_GetWindowID(window_.get())) {
                            should_close_ = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        [[nodiscard]] bool should_close() const {
            return should_close_;
        }

        [[nodiscard]] SDL_Window* get_native_handle() const {
            return window_.get();
        }


    private:
        WindowPtr window_;
        bool should_close_{false};
    };
}