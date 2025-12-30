//
// Created by fcors on 12/19/2025.
//
module;

#include <retro/core/exports.h>

export module retro.renderer:window;

import std;
import retro.core;
import :vulkan_viewport;
import sdl;

namespace retro
{
    struct WindowDeleter
    {
        inline void operator()(sdl::SDL_Window *window) const noexcept
        {
            sdl::DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<sdl::SDL_Window, WindowDeleter>;

    export class RETRO_API Window final : public VulkanViewport
    {
      public:
        inline Window(const int32 width, const int32 height, const CStringView title)
        {
            window_ = WindowPtr{
                sdl::CreateWindow(title.data(), width, height, sdl::WindowFlags::RESIZABLE | sdl::WindowFlags::VULKAN),
            };

            if (window_ == nullptr)
            {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + sdl::GetError()};
            }
        }

        [[nodiscard]] inline sdl::SDL_Window *native_handle() const
        {
            return window_.get();
        }

        // NOLINTNEXTLINE
        inline void set_title(const CStringView title)
        {
            sdl::SetWindowTitle(window_.get(), title.data());
        }

        [[nodiscard]] vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const override;

        [[nodiscard]] inline Vector2u size() const override
        {
            int w = 0;
            int h = 0;
            sdl::GetWindowSizeInPixels(window_.get(), &w, &h);
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