/**
 * @file command_pool.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.command_pool;

import vulkan_hpp;

namespace retro
{
    export struct CommandPoolConfig
    {
        vk::Device device = nullptr;
        std::uint32_t queue_family_idx = vk::QueueFamilyIgnored;
        std::uint32_t buffer_count = 0; // typically MAX_FRAMES_IN_FLIGHT
    };

    export class VulkanCommandPool
    {
      public:
        explicit VulkanCommandPool(const CommandPoolConfig &cfg);

        [[nodiscard]] inline vk::CommandPool pool() const noexcept
        {
            return pool_.get();
        }
        [[nodiscard]] inline const std::vector<vk::UniqueCommandBuffer> &buffers() const noexcept
        {
            return buffers_;
        }
        [[nodiscard]] inline vk::CommandBuffer buffer_at(const size_t index) const noexcept
        {
            return buffers_[index].get();
        }

      private:
        vk::UniqueCommandPool pool_{nullptr};
        std::vector<vk::UniqueCommandBuffer> buffers_{};
    };
} // namespace retro
