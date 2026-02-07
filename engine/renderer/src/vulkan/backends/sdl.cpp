/**
 * @file sdl.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL_vulkan.h>

module retro.renderer.vulkan.backends.sdl;

namespace retro::sdl
{
    std::span<const char *const> get_required_instance_extensions()
    {
        std::uint32_t count = 0;
        auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
        if (names == nullptr)
        {
            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
        }

        // ReSharper disable once CppDFALocalValueEscapesFunction
        return std::span{names, count};
    }

    vk::UniqueSurfaceKHR create_surface(const vk::Instance instance, void *window)
    {
        vk::SurfaceKHR::CType surface;
        if (!SDL_Vulkan_CreateSurface(static_cast<SDL_Window *>(window), instance, nullptr, &surface))
        {
            throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
        }

        return vk::UniqueSurfaceKHR{surface, instance};
    }
} // namespace retro::sdl
