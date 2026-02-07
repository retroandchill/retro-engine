/**
 * @file device.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.scopes.device;

import vulkan_hpp;
import retro.core.di;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.pipeline;

namespace retro
{
    export struct VulkanDeviceCreateInfo
    {
        vk::Instance instance = nullptr;
        vk::SurfaceKHR surface = nullptr;
        bool require_swapchain = true;
    };

    export class VulkanDevice
    {
        explicit VulkanDevice(vk::PhysicalDevice physical_device,
                              vk::UniqueDevice device,
                              std::uint32_t graphics_family_index,
                              std::uint32_t present_family_index,
                              vk::UniqueCommandPool command_pool,
                              vk::UniqueSampler linear_sampler);

      public:
        static std::unique_ptr<VulkanDevice> create(const VulkanDeviceCreateInfo &create_info);

        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands() const;

        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

      private:
        vk::PhysicalDevice physical_device_{};
        std::uint32_t graphics_family_index_{vk::QueueFamilyIgnored};
        std::uint32_t present_family_index_{vk::QueueFamilyIgnored};
        vk::UniqueDevice device_{};
        vk::Queue graphics_queue_{};
        vk::Queue present_queue_{};
        VulkanBufferManager buffer_manager_;
        VulkanPipelineManager pipeline_manager_;
        vk::UniqueCommandPool command_pool_;
        vk::UniqueSampler linear_sampler_;
    };
} // namespace retro
