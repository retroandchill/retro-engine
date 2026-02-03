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
    std::uint32_t find_memory_type(vk::PhysicalDevice physical_device,
                                   const std::uint32_t type_filter,
                                   vk::MemoryPropertyFlags properties)
    {
        const auto mem_properties = physical_device.getMemoryProperties();

        for (std::uint32_t i = 0; i < mem_properties.memoryTypeCount; ++i)
        {
            if (type_filter & (1 << i) && (mem_properties.memoryTypes[i].propertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw std::runtime_error("VulkanBufferManager: failed to find suitable memory type!");
    }

    VulkanBufferManager::VulkanBufferManager(const VulkanDevice &device, std::size_t pool_size)
        : physical_device_(device.physical_device()), device_{device.device()}, pool_size_{pool_size}
    {
        const vk::BufferCreateInfo buffer_info{
            .size = pool_size_,
            .usage = vk::BufferUsageFlagBits::eVertexBuffer | vk::BufferUsageFlagBits::eIndexBuffer |
                     vk::BufferUsageFlagBits::eStorageBuffer,
        };
        buffer_ = device_.createBufferUnique(buffer_info);

        const auto mem_reqs = device_.getBufferMemoryRequirements(buffer_.get());

        const vk::MemoryAllocateInfo alloc_info{
            .allocationSize = mem_reqs.size,
            .memoryTypeIndex =
                find_memory_type(physical_device_,
                                 mem_reqs.memoryTypeBits,
                                 vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent)};

        memory_ = device_.allocateMemoryUnique(alloc_info);
        device_.bindBufferMemory(buffer_.get(), memory_.get(), 0);
        mapped_ptr_ = device_.mapMemory(memory_.get(), 0, pool_size_);
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
