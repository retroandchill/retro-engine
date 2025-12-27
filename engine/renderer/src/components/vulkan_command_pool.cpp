//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    void VulkanCommandPool::recreate(const CommandPoolConfig &cfg)
    {
        reset();
        create(cfg);
    }

    void VulkanCommandPool::reset() noexcept
    {
        if (device_ != VK_NULL_HANDLE)
        {
            if (!buffers_.empty())
            {
                vkFreeCommandBuffers(device_, pool_, static_cast<uint32_t>(buffers_.size()), buffers_.data());
                buffers_.clear();
            }

            if (pool_ != VK_NULL_HANDLE)
            {
                vkDestroyCommandPool(device_, pool_, nullptr);
                pool_ = VK_NULL_HANDLE;
            }
        }
        device_ = VK_NULL_HANDLE;
    }

    void VulkanCommandPool::create(const CommandPoolConfig &cfg)
    {
        if (!cfg.device || cfg.queue_family_idx == VK_QUEUE_FAMILY_IGNORED || cfg.buffer_count == 0)
        {
            throw std::runtime_error{"VulkanCommandPool: invalid config"};
        }

        device_ = cfg.device;

        VkCommandPoolCreateInfo pool_info{};
        pool_info.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
        pool_info.queueFamilyIndex = cfg.queue_family_idx;
        pool_info.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

        if (vkCreateCommandPool(device_, &pool_info, nullptr, &pool_) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanCommandPool: failed to create command pool"};
        }

        buffers_.resize(cfg.buffer_count);

        VkCommandBufferAllocateInfo alloc_info{};
        alloc_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
        alloc_info.commandPool = pool_;
        alloc_info.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
        alloc_info.commandBufferCount = cfg.buffer_count;

        if (vkAllocateCommandBuffers(device_, &alloc_info, buffers_.data()) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanCommandPool: failed to allocate command buffers"};
        }
    }
} // namespace retro
