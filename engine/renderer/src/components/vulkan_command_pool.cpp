/**
 * @file vulkan_command_pool.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
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
} // namespace retro
