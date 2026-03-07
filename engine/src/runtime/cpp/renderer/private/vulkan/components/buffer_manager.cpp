/**
 * @file buffer_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.components.buffer_manager;

namespace retro
{
    VulkanBufferManager::VulkanBufferManager(vk::UniqueBuffer buffer,
                                             vk::UniqueDeviceMemory memory,
                                             void *mapped_ptr,
                                             const std::size_t pool_size)
        : buffer_{std::move(buffer)}, memory_{std::move(memory)}, mapped_ptr_{mapped_ptr}, pool_size_{pool_size}
    {
    }

    TransientAllocation VulkanBufferManager::allocate_transient(const std::size_t size, vk::BufferUsageFlags usage)
    {
        // Align offset (e.g., 16 bytes for safety)
        current_offset_ = current_offset_ + 15 & ~15;

        if (current_offset_ + size > pool_size_)
        {
            throw std::bad_alloc{};
        }

        const TransientAllocation allocation{.buffer = buffer_.get(),
                                             .mapped_data = static_cast<std::byte *>(mapped_ptr_) + current_offset_,
                                             .offset = current_offset_};

        current_offset_ += size;
        return allocation;
    }

    void VulkanBufferManager::reset()
    {
        current_offset_ = 0;
    }
} // namespace retro
