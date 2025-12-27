//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    VulkanCommandPool::VulkanCommandPool(const CommandPoolConfig &cfg)
    {
        if (!cfg.device || cfg.queue_family_idx == VK_QUEUE_FAMILY_IGNORED || cfg.buffer_count == 0)
        {
            throw std::runtime_error{"VulkanCommandPool: invalid config"};
        }

        VkCommandPoolCreateInfo pool_info{};
        pool_info.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
        pool_info.queueFamilyIndex = cfg.queue_family_idx;
        pool_info.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

        pool_ = cfg.device.createCommandPoolUnique(pool_info);


        buffers_.resize(cfg.buffer_count);

        vk::CommandBufferAllocateInfo alloc_info{
            pool_.get(),
            vk::CommandBufferLevel::ePrimary,
            cfg.buffer_count
        };

        buffers_ = cfg.device.allocateCommandBuffersUnique(alloc_info);
    }
} // namespace retro
