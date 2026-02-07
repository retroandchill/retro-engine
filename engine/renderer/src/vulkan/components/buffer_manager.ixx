/**
 * @file buffer_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.buffer_manager;

import retro.core.di;
import vulkan_hpp;

namespace retro
{
    export struct TransientAllocation
    {
        vk::Buffer buffer;
        void *mapped_data;
        size_t offset;
    };

    export class VulkanBufferManager
    {
        constexpr static std::size_t DEFAULT_POOL_SIZE = 1024 * 1024 * 10;

        explicit VulkanBufferManager(vk::UniqueBuffer buffer,
                                     vk::UniqueDeviceMemory memory,
                                     void *mapped_ptr,
                                     std::size_t pool_size);

      public:
        static VulkanBufferManager create(vk::Device device,
                                          vk::PhysicalDevice physical_device,
                                          std::size_t pool_size = DEFAULT_POOL_SIZE);

        TransientAllocation allocate_transient(std::size_t size, vk::BufferUsageFlags usage);

        void reset();

      private:
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        void *mapped_ptr_ = nullptr;
        std::size_t pool_size_{DEFAULT_POOL_SIZE};
        std::size_t current_offset_ = 0;
    };

    export std::uint32_t find_memory_type(vk::PhysicalDevice physical_device,
                                          const std::uint32_t type_filter,
                                          vk::MemoryPropertyFlags properties);
} // namespace retro
