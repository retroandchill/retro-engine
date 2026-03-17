/**
 * @file buffer_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.buffer_manager;

import vulkan;

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
      public:
        explicit VulkanBufferManager(vk::UniqueBuffer buffer,
                                     vk::UniqueDeviceMemory memory,
                                     void *mapped_ptr,
                                     std::size_t pool_size);

        TransientAllocation allocate_transient(std::size_t size, vk::BufferUsageFlags usage);

        void reset();

      private:
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        void *mapped_ptr_ = nullptr;
        std::size_t pool_size_ = 0;
        std::size_t current_offset_ = 0;
    };
} // namespace retro
