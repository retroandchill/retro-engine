//
// Created by fcors on 12/19/2025.
//
module;

#include <retro/core/exports.h>

export module retro.renderer:window;

import std;
import retro.core;
import :vulkan_viewport;
import vulkan_hpp;
import sdl;

namespace retro
{
    export class RETRO_API GameWindow final : public VulkanViewport
    {
      public:
        inline GameWindow(const int32 width, const int32 height, const CStringView title) :
            window_{sdl::create_window(title.data(), width, height, sdl::WINDOW_RESIZABLE | sdl::WINDOW_VULKAN)}
        {
        }

        [[nodiscard]] inline sdl::WindowView get() const
        {
            return window_.get();
        }

        // NOLINTNEXTLINE
        inline void set_title(const CStringView title)
        {
            window_->set_title(title.data());
        }

        [[nodiscard]] inline vk::UniqueSurfaceKHR create_surface(const vk::Instance instance) const override
        {
            return sdl::vulkan::create_surface(window_.get(), instance);
        }

        [[nodiscard]] inline Size2<uint32> size() const override
        {
            auto [w, h] = window_->size();
            return {static_cast<uint32>(w), static_cast<uint32>(h)};
        }

        [[nodiscard]] inline friend bool operator==(const GameWindow &a, const GameWindow &b) noexcept
        {
            return a.get() == b.get();
        }

        [[nodiscard]] inline friend bool operator==(const GameWindow &a, std::nullptr_t) noexcept
        {
            return a.get() == nullptr;
        }

      private:
        sdl::Window window_;
    };
} // namespace retro