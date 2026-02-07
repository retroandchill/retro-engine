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

module retro.renderer.vulkan.scope.device;

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

        Optional<std::uint32_t> pick_graphics_queue_family(vk::PhysicalDevice physical_device)
        {
            for (const auto [i, family] : physical_device.getQueueFamilyProperties() | std::views::enumerate)
            {
                if (family.queueFlags & vk::QueueFlagBits::eGraphics)
                {
                    return i;
                }
            }

            return std::nullopt;
        }

        std::pair<vk::PhysicalDevice, std::uint32_t> pick_physical_device(const vk::Instance instance,
                                                                          bool require_swapchain)
        {
            for (const auto devices = instance.enumeratePhysicalDevices(); const auto dev : devices)
            {
                auto graphics_queue_family = pick_graphics_queue_family(dev);
                if (!graphics_queue_family.has_value())
                {
                    continue;
                }

                if (!has_required_device_extensions(dev, require_swapchain))
                {
                    continue;
                }

                return std::make_pair(dev, *graphics_queue_family);
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
    } // namespace

    VulkanDevice::VulkanDevice(const vk::PhysicalDevice physical_device,
                               vk::UniqueDevice device,
                               const std::uint32_t graphics_family_index,
                               vk::UniqueCommandPool command_pool)
        : physical_device_(physical_device), graphics_family_index_(graphics_family_index),
          present_family_index_{graphics_family_index_}, device_(std::move(device)),
          graphics_queue_{device_.get().getQueue(graphics_family_index_, 0)},
          present_queue_{device_.get().getQueue(present_family_index_, 0)},
          buffer_manager_{VulkanBufferManager::create(device_.get(), physical_device_)},
          pipeline_manager_{device_.get()}, command_pool_{std::move(command_pool)}

    {
    }

    VulkanDevice VulkanDevice::create(const VulkanDeviceCreateInfo &create_info)
    {
        if (create_info.instance == nullptr)
        {
            throw std::runtime_error{"VulkanDevice: instance is null"};
        }

        auto [physical_device, graphics_family] =
            pick_physical_device(create_info.instance, create_info.require_swapchain);

        auto device = create_logical_device(physical_device, graphics_family, create_info.require_swapchain);

        const vk::CommandPoolCreateInfo pool_info{.flags = vk::CommandPoolCreateFlagBits::eResetCommandBuffer,
                                                  .queueFamilyIndex = graphics_family};

        auto command_pool = device->createCommandPoolUnique(pool_info);
        return VulkanDevice{physical_device, std::move(device), graphics_family, std::move(command_pool)};
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

    std::vector<vk::UniqueCommandBuffer> VulkanDevice::create_command_buffers(const std::uint32_t count) const
    {
        return device_->allocateCommandBuffersUnique(
            vk::CommandBufferAllocateInfo{.commandPool = command_pool_.get(),
                                          .level = vk::CommandBufferLevel::ePrimary,
                                          .commandBufferCount = count});
    }
} // namespace retro
