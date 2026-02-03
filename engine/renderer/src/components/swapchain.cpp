/**
 * @file swapchain.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer;

namespace retro
{
    VulkanSwapchain::VulkanSwapchain(const SwapchainConfig &config)
    {
        // 1) Query surface capabilities
        auto capabilities = config.physical_device.getSurfaceCapabilitiesKHR(config.surface);

        vk::Extent2D desired_extent{config.width, config.height};

        vk::Extent2D actual_extent{std::clamp<std::uint32_t>(desired_extent.width,
                                                             capabilities.minImageExtent.width,
                                                             capabilities.maxImageExtent.width),
                                   std::clamp<std::uint32_t>(desired_extent.height,
                                                             capabilities.minImageExtent.height,
                                                             capabilities.maxImageExtent.height)};

        auto formats = config.physical_device.getSurfaceFormatsKHR(config.surface);
        if (formats.size() == 0)
        {
            throw std::runtime_error{"VulkanSwapchain: no surface formats"};
        }

        auto chosen_format = formats[0];
        for (const auto &f : formats)
        {
            if (f.format == vk::Format::eB8G8R8A8Srgb && f.colorSpace == vk::ColorSpaceKHR::eSrgbNonlinear)
            {
                chosen_format = f;
                break;
            }
        }

        // 3) Choose present mode
        auto present_modes = config.physical_device.getSurfacePresentModesKHR(config.surface);
        if (present_modes.size() == 0)
        {
            throw std::runtime_error{"VulkanSwapchain: no present modes"};
        }

        auto chosen_present_mode = vk::PresentModeKHR::eFifo; // always available

        // 4) Choose image count
        std::uint32_t image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        // 5) Create swapchain
        vk::SwapchainCreateInfoKHR ci{
            .surface = config.surface,
            .minImageCount = image_count,
            .imageFormat = chosen_format.format,
            .imageColorSpace = chosen_format.colorSpace,
            .imageExtent = actual_extent,
            .imageArrayLayers = 1,
            .imageUsage = vk::ImageUsageFlagBits::eColorAttachment,
        };

        std::array queue_family_indices = {config.graphics_family, config.present_family};

        if (config.graphics_family != config.present_family)
        {
            ci.imageSharingMode = vk::SharingMode::eConcurrent;
            ci.queueFamilyIndexCount = queue_family_indices.size();
            ci.pQueueFamilyIndices = queue_family_indices.data();
        }
        else
        {
            ci.imageSharingMode = vk::SharingMode::eExclusive;
        }

        ci.preTransform = capabilities.currentTransform;
        ci.compositeAlpha = vk::CompositeAlphaFlagBitsKHR::eOpaque;
        ci.presentMode = chosen_present_mode;
        ci.clipped = vk::True;
        ci.oldSwapchain = config.old_swapchain;

        swapchain_ = config.device.createSwapchainKHRUnique(ci);

        // 6) Get images
        std::uint32_t actual_image_count = 0;
        images_ = config.device.getSwapchainImagesKHR(swapchain_.get());

        format_ = chosen_format.format;
        extent_ = actual_extent;

        // 7) Create image views
        create_image_views(config.device);
    }

    void VulkanSwapchain::create_image_views(const vk::Device device)
    {
        image_views_ =
            images_ |
            std::views::transform(
                [device, this](const vk::Image image)
                {
                    return device.createImageViewUnique(vk::ImageViewCreateInfo{
                        .image = image,
                        .viewType = vk::ImageViewType::e2D,
                        .format = format_,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
                    });
                }) |
            std::ranges::to<std::vector>();
    }
} // namespace retro
