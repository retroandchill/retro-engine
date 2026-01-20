/**
 * @file vulkan_buffer_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:vulkan_buffer_manager;

import std;
import vulkan_hpp;
import :components.vulkan_device;
import retro.runtime;

namespace retro
{
    export struct TransientAllocation
    {
        vk::Buffer buffer;
        void *mapped_data;
        size_t offset;
    };

    export class RETRO_API VulkanBufferManager
    {
        explicit VulkanBufferManager(const VulkanDevice &device, usize pool_size);

      public:
        static void initialize(const VulkanDevice &device, usize pool_size = PipelineManager::DEFAULT_POOL_SIZE);

        static void shutdown();

        static VulkanBufferManager &instance();

        TransientAllocation allocate_transient(usize size, vk::BufferUsageFlags usage);

        void reset();

      private:
        [[nodiscard]] uint32 find_memory_type(uint32 type_filter, vk::MemoryPropertyFlags properties) const;

        vk::PhysicalDevice physical_device_;
        vk::Device device_;
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        void *mapped_ptr_ = nullptr;
        usize pool_size_{PipelineManager::DEFAULT_POOL_SIZE};
        usize current_offset_ = 0;

        static std::unique_ptr<VulkanBufferManager> instance_;
    };

    export class RETRO_API VulkanBufferManagerScope
    {
      public:
        explicit inline VulkanBufferManagerScope(const VulkanDevice &device,
                                                 const usize pool_size = PipelineManager::DEFAULT_POOL_SIZE)
        {
            VulkanBufferManager::initialize(device, pool_size);
        }

        VulkanBufferManagerScope(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope(VulkanBufferManagerScope &&) noexcept = delete;

        inline ~VulkanBufferManagerScope()
        {
            VulkanBufferManager::shutdown();
        }

        VulkanBufferManagerScope &operator=(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope &operator=(VulkanBufferManagerScope &&) noexcept = delete;
    };
} // namespace retro
