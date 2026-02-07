/**
 * @file device.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.scopes.device;

import retro.core.containers.optional;

namespace retro
{
    namespace
    {
        bool has_required_device_extensions(vk::PhysicalDevice physical_device, bool require_swapchain)
        {
            if (!require_swapchain)
            {
                return true;
            }

            constexpr std::array required_exts = {vk::KHRSwapchainExtensionName};
            const auto available = physical_device.enumerateDeviceExtensionProperties();

            const auto has_ext = [&](const char *name)
            {
                return std::ranges::any_of(available,
                                           [&](const vk::ExtensionProperties &ep)
                                           { return std::string_view{ep.extensionName} == name; });
            };

            return std::ranges::all_of(required_exts, has_ext);
        }

        Optional<std::pair<std::uint32_t, std::uint32_t>> pick_queue_families(vk::PhysicalDevice device,
                                                                              vk::SurfaceKHR surface)
        {
            auto families = device.getQueueFamilyProperties();

            std::uint32_t out_graphics_family = vk::QueueFamilyIgnored;
            std::uint32_t out_present_family = vk::QueueFamilyIgnored;
            ;

            for (std::uint32_t i = 0; i < families.size(); ++i)
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

                if (out_graphics_family != vk::QueueFamilyIgnored && out_present_family != vk::QueueFamilyIgnored)
                {
                    break;
                }
            }

            if (out_graphics_family == vk::QueueFamilyIgnored || out_present_family == vk::QueueFamilyIgnored)
            {
                return std::nullopt;
            }

            // Check swapchain support (at least one format & present mode)
            std::uint32_t format_count = 0;
            auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
            if (res != vk::Result::eSuccess || format_count == 0)
            {
                return std::nullopt;
            }

            std::uint32_t present_mode_count = 0;
            res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
            if (res != vk::Result::eSuccess || present_mode_count == 0)
            {
                return std::nullopt;
            }

            return std::make_pair(out_graphics_family, out_present_family);
        }

        std::tuple<vk::PhysicalDevice, std::uint32_t, std::uint32_t> pick_physical_device(const vk::Instance instance,
                                                                                          const vk::SurfaceKHR surface,
                                                                                          bool require_swapchain)
        {
            for (const auto devices = instance.enumeratePhysicalDevices(); const auto dev : devices)
            {
                auto queue_families = pick_queue_families(dev, surface);
                if (!queue_families.has_value())
                {
                    continue;
                }

                if (!has_required_device_extensions(dev, require_swapchain))
                {
                    continue;
                }

                auto [graphics_family, present_family] = queue_families.value();
                return std::make_tuple(dev, graphics_family, present_family);
            }

            throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_logical_device(vk::PhysicalDevice physical_device,
                                               std::uint32_t graphics_family_index,
                                               bool require_swapchain)
        {
            std::vector<const char *> device_exts;
            if (require_swapchain)
            {
                device_exts.push_back(vk::KHRSwapchainExtensionName);
            }

            constexpr float queue_priority = 1.0f;
            const vk::DeviceQueueCreateInfo queue_info{
                .queueFamilyIndex = graphics_family_index,
                .queueCount = 1,
                .pQueuePriorities = &queue_priority,
            };

            vk::PhysicalDeviceFeatures device_features{};

            const vk::DeviceCreateInfo create_info{
                .queueCreateInfoCount = 1,
                .pQueueCreateInfos = &queue_info,
                .enabledExtensionCount = static_cast<std::uint32_t>(device_exts.size()),
                .ppEnabledExtensionNames = device_exts.data(),
                .pEnabledFeatures = &device_features,
            };

            return physical_device.createDeviceUnique(create_info, nullptr);
        }

        vk::UniqueSampler create_linear_sampler(const vk::Device device)
        {
            constexpr vk::SamplerCreateInfo sampler_info{
                .magFilter = vk::Filter::eLinear,
                .minFilter = vk::Filter::eLinear,
                .mipmapMode = vk::SamplerMipmapMode::eLinear,
                .addressModeU = vk::SamplerAddressMode::eClampToEdge,
                .addressModeV = vk::SamplerAddressMode::eClampToEdge,
                .addressModeW = vk::SamplerAddressMode::eClampToEdge,
                .mipLodBias = 0.0f,
                .anisotropyEnable = vk::False,
                .maxAnisotropy = 1.0f,
                .compareEnable = vk::False,
                .compareOp = vk::CompareOp::eAlways,
                .minLod = 0.0f,
                .maxLod = 0.0f,
                .borderColor = vk::BorderColor::eIntOpaqueBlack,
                .unnormalizedCoordinates = vk::False,
            };

            return device.createSamplerUnique(sampler_info);
        }
    } // namespace

    VulkanDevice::VulkanDevice(const vk::PhysicalDevice physical_device,
                               vk::UniqueDevice device,
                               const std::uint32_t graphics_family_index,
                               const std::uint32_t present_family_index,
                               vk::UniqueCommandPool command_pool,
                               vk::UniqueSampler linear_sampler)
        : physical_device_(physical_device), graphics_family_index_(graphics_family_index),
          present_family_index_(present_family_index), device_(std::move(device)),
          graphics_queue_{device_.get().getQueue(graphics_family_index_, 0)},
          present_queue_{device_.get().getQueue(present_family_index_, 0)},
          buffer_manager_{VulkanBufferManager::create(device_.get(), physical_device_)},
          pipeline_manager_{device_.get()}, command_pool_{std::move(command_pool)},
          linear_sampler_{std::move(linear_sampler)}

    {
    }

    std::unique_ptr<VulkanDevice> VulkanDevice::create(const VulkanDeviceCreateInfo &create_info)
    {
        if (create_info.instance == nullptr)
        {
            throw std::runtime_error{"VulkanDevice: instance is null"};
        }

        auto [physical_device, graphics_family, present_family] =
            pick_physical_device(create_info.instance, create_info.surface, create_info.require_swapchain);

        auto device = create_logical_device(physical_device, graphics_family, create_info.require_swapchain);

        const vk::CommandPoolCreateInfo pool_info{.flags = vk::CommandPoolCreateFlagBits::eResetCommandBuffer,
                                                  .queueFamilyIndex = graphics_family};

        auto command_pool = device->createCommandPoolUnique(pool_info);

        auto linear_sampler = create_linear_sampler(device.get());

        return std::make_unique<VulkanDevice>(physical_device,
                                              std::move(device),
                                              graphics_family,
                                              present_family,
                                              std::move(command_pool),
                                              std::move(linear_sampler));
    }

    vk::UniqueCommandBuffer VulkanDevice::begin_one_shot_commands() const
    {
        const vk::CommandBufferAllocateInfo alloc_info{
            .commandPool = command_pool_.get(),
            .level = vk::CommandBufferLevel::ePrimary,
            .commandBufferCount = 1,
        };

        auto buffers = device_->allocateCommandBuffersUnique(alloc_info);
        vk::UniqueCommandBuffer cmd = std::move(buffers.front());

        constexpr vk::CommandBufferBeginInfo begin_info{
            .flags = vk::CommandBufferUsageFlagBits::eOneTimeSubmit,
        };

        cmd->begin(begin_info);
        return cmd;
    }

    void VulkanDevice::end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const
    {
        cmd->end();

        constexpr vk::FenceCreateInfo fence_info{};
        vk::UniqueFence fence = device_->createFenceUnique(fence_info);

        vk::CommandBuffer raw_cmd = cmd.get();
        const vk::SubmitInfo submit_info{
            .commandBufferCount = 1,
            .pCommandBuffers = &raw_cmd,
        };

        if (graphics_queue_.submit(1, &submit_info, fence.get()) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit one-shot command buffer"};
        }

        // Simple and safe for asset loading. If you later want async streaming, swap this for a timeline semaphore.
        if (device_->waitForFences(1, &fence.get(), vk::True, std::numeric_limits<std::uint64_t>::max()) !=
            vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed waiting for one-shot fence"};
        }
    }
} // namespace retro
