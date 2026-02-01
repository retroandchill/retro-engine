/**
 * @file components.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer;

import vulkan_hpp;

namespace retro
{
    VulkanCommandPool::VulkanCommandPool(const CommandPoolConfig &cfg)
    {
        if (!cfg.device || cfg.queue_family_idx == vk::QueueFamilyIgnored || cfg.buffer_count == 0)
        {
            throw std::runtime_error{"VulkanCommandPool: invalid config"};
        }

        vk::CommandPoolCreateInfo pool_info{vk::CommandPoolCreateFlagBits::eResetCommandBuffer, cfg.queue_family_idx};

        pool_ = cfg.device.createCommandPoolUnique(pool_info);

        buffers_.resize(cfg.buffer_count);

        vk::CommandBufferAllocateInfo alloc_info{pool_.get(), vk::CommandBufferLevel::ePrimary, cfg.buffer_count};

        buffers_ = cfg.device.allocateCommandBuffersUnique(alloc_info);
    }

    vk::CommandBuffer VulkanCommandPool::begin_single_time_commands()
    {
        return vk::CommandBuffer{};
    }

    void VulkanCommandPool::end_single_time_commands(vk::CommandBuffer command_buffer, vk::Queue queue)
    {
    }

    VulkanDevice::VulkanDevice(vk::Instance instance, vk::SurfaceKHR surface)
        : physical_device_{pick_physical_device(instance, surface, graphics_family_index_, present_family_index_)},
          device_{create_device(physical_device_, graphics_family_index_, present_family_index_)},
          graphics_queue_{device_->getQueue(graphics_family_index_, 0)},
          present_queue_{device_->getQueue(present_family_index_, 0)}
    {
    }

    vk::PhysicalDevice VulkanDevice::pick_physical_device(vk::Instance instance,
                                                          vk::SurfaceKHR surface,
                                                          uint32 &out_graphics_family,
                                                          uint32 &out_present_family)
    {
        auto devices = instance.enumeratePhysicalDevices();

        for (auto dev : devices)
        {
            uint32 graphics_family = std::numeric_limits<uint32>::max();
            uint32 present_family = std::numeric_limits<uint32>::max();

            if (is_device_suitable(dev, surface, graphics_family, present_family))
            {
                out_graphics_family = graphics_family;
                out_present_family = present_family;
                return dev;
            }
        }

        throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
    }

    bool VulkanDevice::is_device_suitable(const vk::PhysicalDevice device,
                                          const vk::SurfaceKHR surface,
                                          uint32 &out_graphics_family,
                                          uint32 &out_present_family)
    {
        auto families = device.getQueueFamilyProperties();

        out_graphics_family = std::numeric_limits<uint32>::max();
        out_present_family = std::numeric_limits<uint32>::max();

        for (uint32 i = 0; i < families.size(); ++i)
        {
            if (families[i].queueFlags & vk::QueueFlagBits::eGraphics)
            {
                out_graphics_family = i;
            }

            vk::Bool32 present_support = vk::False;
            auto res = device.getSurfaceSupportKHR(i, surface, &present_support);
            if (res == vk::Result::eSuccess && present_support == vk::True)
            {
                out_present_family = i;
            }

            if (out_graphics_family != std::numeric_limits<uint32>::max() &&
                out_present_family != std::numeric_limits<uint32>::max())
            {
                break;
            }
        }

        if (out_graphics_family == std::numeric_limits<uint32>::max() ||
            out_present_family == std::numeric_limits<uint32>::max())
        {
            return false;
        }

        // Check swapchain support (at least one format & present mode)
        uint32 format_count = 0;
        auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
        if (res != vk::Result::eSuccess || format_count == 0)
        {
            return false;
        }

        uint32 present_mode_count = 0;
        res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
        if (res != vk::Result::eSuccess || present_mode_count == 0)
        {
            return false;
        }

        return true;
    }

    vk::UniqueDevice VulkanDevice::create_device(const vk::PhysicalDevice physical_device,
                                                 const uint32 graphics_family,
                                                 const uint32 present_family)
    {
        // Required device extensions
        constexpr std::array DEVICE_EXTENSIONS = {vk::KHRSwapchainExtensionName};

        std::set unique_families{graphics_family, present_family};

        float queue_priority = 1.0f;
        std::vector<vk::DeviceQueueCreateInfo> queue_infos;
        queue_infos.reserve(unique_families.size());

        for (uint32 family : unique_families)
        {
            queue_infos.emplace_back(vk::DeviceQueueCreateFlags{}, family, 1, &queue_priority);
        }

        vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

        vk::DeviceCreateInfo create_info{{},
                                         static_cast<uint32>(queue_infos.size()),
                                         queue_infos.data(),
                                         0,
                                         nullptr,
                                         DEVICE_EXTENSIONS.size(),
                                         DEVICE_EXTENSIONS.data(),
                                         &device_features};

        return physical_device.createDeviceUnique(create_info, nullptr);
    }

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
        ci.oldSwapchain = config.old_swapchain;

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

    VulkanSyncObjects::VulkanSyncObjects(const SyncConfig &cfg)
    {
        if (!cfg.device || cfg.frames_in_flight == 0)
        {
            throw std::runtime_error{"VulkanSyncObjects: invalid config"};
        };

        image_available_.reserve(cfg.frames_in_flight);
        render_finished_.reserve(cfg.frames_in_flight);
        in_flight_.reserve(cfg.frames_in_flight);
        descriptor_pools_.reserve(cfg.frames_in_flight);

        constexpr vk::SemaphoreCreateInfo sem_info{};

        constexpr vk::FenceCreateInfo fence_info{vk::FenceCreateFlagBits::eSignaled};

        vk::DescriptorPoolSize pool_size{vk::DescriptorType::eStorageBuffer, 256};
        const vk::DescriptorPoolCreateInfo pool_info{{}, 256, 1, &pool_size};

        for (size_t i = 0; i < cfg.frames_in_flight; ++i)
        {
            image_available_.emplace_back(cfg.device.createSemaphoreUnique(sem_info));
            in_flight_.emplace_back(cfg.device.createFenceUnique(fence_info));
            descriptor_pools_.emplace_back(cfg.device.createDescriptorPoolUnique(pool_info));
        }

        for (size_t i = 0; i < cfg.swapchain_image_count; ++i)
        {
            render_finished_.emplace_back(cfg.device.createSemaphoreUnique(sem_info));
        }
    }
} // namespace retro
