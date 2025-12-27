//
// Created by fcors on 12/19/2025.
//
module;

#include <SDL3/SDL_vulkan.h>

export module retro.renderer:window;

import std;
import retro.core;

namespace retro
{
    struct WindowDeleter
    {
        inline void operator()(SDL_Window *window) const noexcept
        {
            SDL_DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<SDL_Window, WindowDeleter>;

    export class Window
    {
      public:
        inline Window(const int32 width, const int32 height, const CStringView title)
        {
            window_ = WindowPtr{SDL_CreateWindow(title.data(), width, height, SDL_WINDOW_RESIZABLE | SDL_WINDOW_VULKAN)};

            if (window_ == nullptr)
            {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + SDL_GetError()};
            }
        }

        [[nodiscard]] inline SDL_Window *native_handle() const
        {
            return window_.get();
        }

        // NOLINTNEXTLINE
        inline void set_title(const CStringView title)
        {
            SDL_SetWindowTitle(window_.get(), title.data());
        }

        [[nodiscard]] inline friend bool operator==(const Window& a, const Window& b) noexcept
        {
            return a.native_handle() == b.native_handle();
        }

        [[nodiscard]] inline friend bool operator==(const Window& a, std::nullptr_t) noexcept
        {
            return a.native_handle() == nullptr;
        }

      private:
        WindowPtr window_;
    };
} // namespace retro