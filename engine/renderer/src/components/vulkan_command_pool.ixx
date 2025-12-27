//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_command_pool;

import std;
import retro.core;

namespace retro
{
    export struct CommandPoolConfig
    {
        VkDevice device = VK_NULL_HANDLE;
        uint32 queue_family_idx = VK_QUEUE_FAMILY_IGNORED;
        uint32 buffer_count = 0; // typically MAX_FRAMES_IN_FLIGHT
    };

    export class RETRO_API VulkanCommandPool
    {
      public:
        explicit inline VulkanCommandPool(const CommandPoolConfig &cfg)
        {
            create(cfg);
        }

        inline ~VulkanCommandPool() noexcept
        {
            reset();
        }

        VulkanCommandPool(const VulkanCommandPool &) = delete;
        VulkanCommandPool &operator=(const VulkanCommandPool &) = delete;
        VulkanCommandPool(VulkanCommandPool &&) = delete;
        VulkanCommandPool &operator=(VulkanCommandPool &&) = delete;

        void recreate(const CommandPoolConfig &cfg);

        void reset() noexcept;

        [[nodiscard]] inline VkCommandPool pool() const noexcept
        {
            return pool_;
        }
        [[nodiscard]] inline const std::vector<VkCommandBuffer> &buffers() const noexcept
        {
            return buffers_;
        }
        [[nodiscard]] inline VkCommandBuffer buffer_at(size_t index) const noexcept
        {
            return buffers_[index];
        }

      private:
        void create(const CommandPoolConfig &cfg);

        VkDevice device_{VK_NULL_HANDLE};
        VkCommandPool pool_{VK_NULL_HANDLE};
        std::vector<VkCommandBuffer> buffers_;
    };
} // namespace retro
