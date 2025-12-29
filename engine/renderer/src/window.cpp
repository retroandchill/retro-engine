//
// Created by fcors on 12/27/2025.
//
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