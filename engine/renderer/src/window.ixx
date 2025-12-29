//
// Created by fcors on 12/19/2025.
//
module;

#include <retro/core/exports.h>
#include <SDL3/SDL_vulkan.h>

export module retro.renderer:window;

import std;
import retro.core;
import :vulkan_viewport;
import vulkan_hpp;

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

    export class RETRO_API Window final : public VulkanViewport
    {
      public:
        inline Window(const int32 width, const int32 height, const CStringView title)
        {
            window_ = WindowPtr{
                SDL_CreateWindow(title.data(), width, height, SDL_WINDOW_RESIZABLE | SDL_WINDOW_VULKAN),
            };

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

        vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const override;

        [[nodiscard]] inline Size2<uint32> size() const override
        {
            int w = 0, h = 0;
            SDL_GetWindowSizeInPixels(window_.get(), &w, &h);
            return {static_cast<uint32>(w), static_cast<uint32>(h)};
        }

        [[nodiscard]] inline friend bool operator==(const Window &a, const Window &b) noexcept
        {
            return a.native_handle() == b.native_handle();
        }

        [[nodiscard]] inline friend bool operator==(const Window &a, std::nullptr_t) noexcept
        {
            return a.native_handle() == nullptr;
        }

      private:
        WindowPtr window_;
    };
} // namespace retro