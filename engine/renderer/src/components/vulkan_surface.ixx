//
// Created by fcors on 12/26/2025.
//
module;

#include <SDL3/SDL_vulkan.h>
#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_surface;

import std;
import :window;
import :components.vulkan_instance;

namespace retro
{
    export class VulkanSurface
    {
    public:
        inline VulkanSurface(const VulkanInstance& instance, const Window& window) : instance_{instance.instance()}
        {
            if (window == nullptr)
            {
                throw std::runtime_error{"VulkanSurface: window is null"};
            }

            if (!SDL_Vulkan_CreateSurface(window.native_handle(), instance_, nullptr, &surface_))
            {
                throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
            }
        }

        inline ~VulkanSurface() noexcept
        {
            vkDestroySurfaceKHR(instance_, surface_, nullptr);
        }

        VulkanSurface(const VulkanSurface&) = delete;
        VulkanSurface& operator=(const VulkanSurface&) = delete;
        VulkanSurface(VulkanSurface&& other) noexcept = delete;
        VulkanSurface& operator=(VulkanSurface&& other) noexcept = delete;

        [[nodiscard]] inline VkSurfaceKHR surface() const noexcept
        {
            return surface_;
        }

        [[nodiscard]] inline VkInstance instance() const noexcept
        {
            return instance_;
        }

    private:
        VkInstance   instance_{VK_NULL_HANDLE};
        VkSurfaceKHR surface_{VK_NULL_HANDLE};
    };
}
