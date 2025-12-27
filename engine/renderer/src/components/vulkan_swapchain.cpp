//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    void VulkanSwapchain::recreate(const SwapchainConfig &cfg)
    {
        reset();
        create(cfg);
    }

    void VulkanSwapchain::reset() noexcept
    {
        for (auto view : image_views_)
        {
            vkDestroyImageView(device_, view, nullptr);
        }
        image_views_.clear();
        images_.clear();

        if (swapchain_ != VK_NULL_HANDLE)
        {
            vkDestroySwapchainKHR(device_, swapchain_, nullptr);
            swapchain_ = VK_NULL_HANDLE;
        }

        device_ = VK_NULL_HANDLE;
        format_ = VK_FORMAT_UNDEFINED;
        extent_ = {};
    }

    void VulkanSwapchain::create(const SwapchainConfig &cfg)
    {
        if (!cfg.physical_device || !cfg.device || !cfg.surface || cfg.width == 0 || cfg.height == 0)
        {
            throw std::runtime_error{"VulkanSwapchain: invalid config"};
        }

        device_ = cfg.device;

        // 1) Query surface capabilities
        VkSurfaceCapabilitiesKHR capabilities{};
        vkGetPhysicalDeviceSurfaceCapabilitiesKHR(cfg.physical_device, cfg.surface, &capabilities);

        VkExtent2D desired_extent{cfg.width, cfg.height};

        VkExtent2D actual_extent{};
        actual_extent.width = std::clamp<uint32>(desired_extent.width,
                                                 capabilities.minImageExtent.width,
                                                 capabilities.maxImageExtent.width);
        actual_extent.height = std::clamp<uint32>(desired_extent.height,
                                                  capabilities.minImageExtent.height,
                                                  capabilities.maxImageExtent.height);

        // 2) Choose surface format
        uint32 format_count = 0;
        vkGetPhysicalDeviceSurfaceFormatsKHR(cfg.physical_device, cfg.surface, &format_count, nullptr);
        if (format_count == 0)
        {
            throw std::runtime_error{"VulkanSwapchain: no surface formats"};
        }

        std::vector<VkSurfaceFormatKHR> formats(format_count);
        vkGetPhysicalDeviceSurfaceFormatsKHR(cfg.physical_device, cfg.surface, &format_count, formats.data());

        VkSurfaceFormatKHR chosen_format = formats[0];
        for (const auto &f : formats)
        {
            if (f.format == VK_FORMAT_B8G8R8A8_SRGB && f.colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
            {
                chosen_format = f;
                break;
            }
        }

        // 3) Choose present mode
        uint32 present_mode_count = 0;
        vkGetPhysicalDeviceSurfacePresentModesKHR(cfg.physical_device, cfg.surface, &present_mode_count, nullptr);
        if (present_mode_count == 0)
        {
            throw std::runtime_error{"VulkanSwapchain: no present modes"};
        }

        std::vector<VkPresentModeKHR> present_modes(present_mode_count);
        vkGetPhysicalDeviceSurfacePresentModesKHR(cfg.physical_device,
                                                  cfg.surface,
                                                  &present_mode_count,
                                                  present_modes.data());

        VkPresentModeKHR chosen_present_mode = VK_PRESENT_MODE_FIFO_KHR; // always available

        // 4) Choose image count
        uint32 image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        // 5) Create swapchain
        VkSwapchainCreateInfoKHR ci{};
        ci.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
        ci.surface = cfg.surface;
        ci.minImageCount = image_count;
        ci.imageFormat = chosen_format.format;
        ci.imageColorSpace = chosen_format.colorSpace;
        ci.imageExtent = actual_extent;
        ci.imageArrayLayers = 1;
        ci.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

        uint32 queue_family_indices[] = {cfg.graphics_family, cfg.present_family};

        if (cfg.graphics_family != cfg.present_family)
        {
            ci.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
            ci.queueFamilyIndexCount = 2;
            ci.pQueueFamilyIndices = queue_family_indices;
        }
        else
        {
            ci.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
        }

        ci.preTransform = capabilities.currentTransform;
        ci.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
        ci.presentMode = chosen_present_mode;
        ci.clipped = VK_TRUE;

        if (vkCreateSwapchainKHR(device_, &ci, nullptr, &swapchain_) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanSwapchain: failed to create swapchain"};
        }

        // 6) Get images
        uint32 actual_image_count = 0;
        vkGetSwapchainImagesKHR(device_, swapchain_, &actual_image_count, nullptr);
        images_.resize(actual_image_count);
        vkGetSwapchainImagesKHR(device_, swapchain_, &actual_image_count, images_.data());

        format_ = chosen_format.format;
        extent_ = actual_extent;

        // 7) Create image views
        create_image_views();
    }

    void VulkanSwapchain::create_image_views()
    {
        image_views_.resize(images_.size());

        for (size_t i = 0; i < images_.size(); ++i)
        {
            VkImageViewCreateInfo view_info{};
            view_info.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
            view_info.image = images_[i];
            view_info.viewType = VK_IMAGE_VIEW_TYPE_2D;
            view_info.format = format_;
            view_info.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
            view_info.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
            view_info.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
            view_info.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
            view_info.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
            view_info.subresourceRange.baseMipLevel = 0;
            view_info.subresourceRange.levelCount = 1;
            view_info.subresourceRange.baseArrayLayer = 0;
            view_info.subresourceRange.layerCount = 1;

            if (vkCreateImageView(device_, &view_info, nullptr, &image_views_[i]) != VK_SUCCESS)
            {
                throw std::runtime_error{"VulkanSwapchain: failed to create image view"};
            }
        }
    }
} // namespace retro
