//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.hpp>

export module retro.renderer:components.vulkan_command_pool;

import std;
import retro.core;

namespace retro
{
    export struct CommandPoolConfig
    {
        vk::Device device = nullptr;
        uint32 queue_family_idx = vk::QueueFamilyIgnored;
        uint32 buffer_count = 0; // typically MAX_FRAMES_IN_FLIGHT
    };

    export class RETRO_API VulkanCommandPool
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
        std::vector<vk::UniqueCommandBuffer> buffers_;
    };

} // namespace retro
