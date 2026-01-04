/**
 * @file window.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

module retro.renderer;

namespace retro
{
    vk::UniqueSurfaceKHR Window::create_surface(vk::Instance instance) const
    {
        vk::SurfaceKHR::CType surface;
        if (!sdl::vulkan::CreateSurface(window_.get(), instance, nullptr, &surface))
        {
            throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
        }

        return vk::UniqueSurfaceKHR{surface, instance};
    }
} // namespace retro
