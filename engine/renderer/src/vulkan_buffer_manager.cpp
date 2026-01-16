/**
 * @file vulkan_buffer_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>
#include <vulkan/vulkan.hpp> // We need this to get past an internal compiler error

module retro.renderer;

namespace retro
{
    std::unique_ptr<VulkanBufferManager> VulkanBufferManager::instance_{nullptr};

    VulkanBufferManager::VulkanBufferManager(const VulkanDevice &device, usize pool_size)
        : physical_device_(device.physical_device()), device_{device.device()}, pool_size_{pool_size}
    {
        const vk::BufferCreateInfo buffer_info{{},
                                               pool_size_,
                                               vk::BufferUsageFlagBits::eVertexBuffer |
                                                   vk::BufferUsageFlagBits::eIndexBuffer};
        buffer_ = device_.createBufferUnique(buffer_info);

        const auto mem_reqs = device_.getBufferMemoryRequirements(buffer_.get());

        const vk::MemoryAllocateInfo alloc_info{
            mem_reqs.size,
            find_memory_type(mem_reqs.memoryTypeBits,
                             vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent)};

        memory_ = device_.allocateMemoryUnique(alloc_info);
        device_.bindBufferMemory(buffer_.get(), memory_.get(), 0);
        mapped_ptr_ = device_.mapMemory(memory_.get(), 0, pool_size_);
    }

    void VulkanBufferManager::initialize(const VulkanDevice &device, const usize pool_size)
    {
        assert(instance_ == nullptr);
        instance_.reset(new VulkanBufferManager{device, pool_size});
    }

    void VulkanBufferManager::shutdown()
    {
        assert(instance_ != nullptr);
        instance_.reset();
    }

    VulkanBufferManager &VulkanBufferManager::instance()
    {
        assert(instance_ != nullptr);
        return *instance_;
    }

    TransientAllocation VulkanBufferManager::allocate_transient(const usize size, vk::BufferUsageFlags usage)
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

    uint32 VulkanBufferManager::find_memory_type(const uint32 type_filter, vk::MemoryPropertyFlags properties) const
    {
        const auto mem_properties = physical_device_.getMemoryProperties();

        for (uint32 i = 0; i < mem_properties.memoryTypeCount; ++i)
        {
            if (type_filter & (1 << i) && (mem_properties.memoryTypes[i].propertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw std::runtime_error("VulkanBufferManager: failed to find suitable memory type!");
    }
} // namespace retro
