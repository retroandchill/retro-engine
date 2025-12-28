//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.hpp>

module retro.renderer;

namespace retro
{
    VulkanSwapchain::VulkanSwapchain(const SwapchainConfig &config)
    {
        // 1) Query surface capabilities
        auto capabilities = config.physical_device.getSurfaceCapabilitiesKHR(config.surface);

        vk::Extent2D desired_extent{config.width, config.height};

        vk::Extent2D actual_extent{std::clamp<uint32>(desired_extent.width,
                                                      capabilities.minImageExtent.width,
                                                      capabilities.maxImageExtent.width),
                                   std::clamp<uint32>(desired_extent.height,
                                                      capabilities.minImageExtent.height,
                                                      capabilities.maxImageExtent.height)};

        // 2) Choose surface format
        uint32 format_count = 0;
        auto formats = config.physical_device.getSurfaceFormatsKHR(config.surface);
        ;
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
        uint32 image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        // 5) Create swapchain
        vk::SwapchainCreateInfoKHR ci{
            {},
            config.surface,
            image_count,
            chosen_format.format,
            chosen_format.colorSpace,
            actual_extent,
            1,
            vk::ImageUsageFlagBits::eColorAttachment,
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

        swapchain_ = config.device.createSwapchainKHRUnique(ci);

        // 6) Get images
        uint32 actual_image_count = 0;
        images_ = config.device.getSwapchainImagesKHR(swapchain_.get());

        format_ = chosen_format.format;
        extent_ = actual_extent;

        // 7) Create image views
        create_image_views(config.device);
    }

    void VulkanSwapchain::create_image_views(const vk::Device device)
    {
        image_views_ = images_ |
                       std::views::transform(
                           [device, this](const vk::Image image)
                           {
                               return device.createImageViewUnique(vk::ImageViewCreateInfo{
                                   {},
                                   image,
                                   vk::ImageViewType::e2D,
                                   format_,
                                   vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                        vk::ComponentSwizzle::eIdentity,
                                                        vk::ComponentSwizzle::eIdentity,
                                                        vk::ComponentSwizzle::eIdentity},
                                   vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
                               });
                           }) |
                       std::ranges::to<std::vector>();
    }
} // namespace retro
