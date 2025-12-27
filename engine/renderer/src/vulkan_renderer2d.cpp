//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>
#include <SDL3/SDL_vulkan.h>

module retro.renderer;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(Window window) : window_{std::move(window)}, instance_{get_instance_create_info()}, surface_{instance_, window_}, device_{instance_, surface_.surface()}
    {
    }

    void VulkanRenderer2D::begin_frame()
    {
    }

    void VulkanRenderer2D::end_frame()
    {
    }

    void VulkanRenderer2D::draw_quad(Vector2 position, Vector2 size, Color color)
    {
    }

    VulkanInstance VulkanRenderer2D::get_instance_create_info()
    {
        VkApplicationInfo app_info{
            .sType = VK_STRUCTURE_TYPE_APPLICATION_INFO,
            .pApplicationName = "Retro Engine",
            .applicationVersion = VK_MAKE_VERSION(1, 0, 0),
            .pEngineName = "Retro Engine",
            .engineVersion = VK_MAKE_VERSION(1, 0, 0),
            .apiVersion = VK_API_VERSION_1_2
        };

        const auto extensions = get_required_instance_extensions();

        const VkInstanceCreateInfo create_info{
            .sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
            .pApplicationInfo = &app_info,
            .enabledExtensionCount = static_cast<uint32>(extensions.size()),
            .ppEnabledExtensionNames = extensions.data()
        };

        return VulkanInstance{create_info};
    }

    std::span<const char *const> VulkanRenderer2D::get_required_instance_extensions()
    {
        // Ask SDL what Vulkan instance extensions are required for this window
        uint32 count = 0;
        auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
        if (names == nullptr)
        {
            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
        }

        return std::span{names, count};
    }
} // namespace retro
