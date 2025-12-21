//
// Created by fcors on 12/19/2025.
//
module;
#include <SDL3/SDL.h>

#include "retro/core/exports.h"

export module retro.platform.window;

import std;
import retro.platform;

namespace retro::platform {
    struct WindowDeleter {
        inline void operator()(SDL_Window* window) const noexcept {
            SDL_DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<SDL_Window, WindowDeleter>;

    export class RETRO_API Window {
    public:
        Window(Platform&, const int width, const int height, const std::string_view title) {
            window = WindowPtr{SDL_CreateWindow(title.data(), width, height, SDL_WINDOW_RESIZABLE)};

            if (window == nullptr) {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + SDL_GetError()};
            }
        }

        void poll_events() {
            SDL_Event event;
            while (SDL_PollEvent(&event)) {
                switch (event.type) {
                    case SDL_EVENT_QUIT:
                        close_requested = true;
                        break;
                    case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                        if (window != nullptr && event.window.windowID == SDL_GetWindowID(window.get())) {
                            close_requested = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        [[nodiscard]] bool should_close() const {
            return close_requested;
        }

        [[nodiscard]] SDL_Window* get_native_handle() const {
            return window.get();
        }


    private:
        WindowPtr window;
        bool close_requested{false};
    };
}