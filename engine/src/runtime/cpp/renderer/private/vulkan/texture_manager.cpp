/**
 * @file texture_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.texture_manager;

import retro.renderer.vulkan.components.buffer_manager;

namespace retro
{
    namespace
    {
        vk::UniqueSampler create_linear_sampler(const vk::Device device)
        {
            constexpr vk::SamplerCreateInfo sampler_info{
                .magFilter = vk::Filter::eNearest,
                .minFilter = vk::Filter::eNearest,
                .mipmapMode = vk::SamplerMipmapMode::eNearest,
                .addressModeU = vk::SamplerAddressMode::eClampToEdge,
                .addressModeV = vk::SamplerAddressMode::eClampToEdge,
                .addressModeW = vk::SamplerAddressMode::eClampToEdge,
                .mipLodBias = 0.0f,
                .anisotropyEnable = vk::False,
                .maxAnisotropy = 1.0f,
                .compareEnable = vk::False,
                .compareOp = vk::CompareOp::eAlways,
                .minLod = 0.0f,
                .maxLod = 0.0f,
                .borderColor = vk::BorderColor::eIntOpaqueBlack,
                .unnormalizedCoordinates = vk::False,
            };

            return device.createSamplerUnique(sampler_info);
        }
    } // namespace

    VulkanTextureManager::VulkanTextureManager(VulkanDevice &device, const vk::CommandPool command_pool)
        : device_{device}, command_pool_{command_pool}, linear_sampler_{create_linear_sampler(device_.device())}
    {
    }

    std::unique_ptr<TextureRenderData> VulkanTextureManager::upload_texture(const ImageData &image_data)
    {
        auto device = device_.device();
        auto image_size = image_data.bytes().size();
        auto image_format = vk::Format::eR8G8B8A8Srgb;

        vk::BufferCreateInfo staging_info{.size = image_size,
                                          .usage = vk::BufferUsageFlagBits::eTransferSrc,
                                          .sharingMode = vk::SharingMode::eExclusive};

        auto staging_buffer = device.createBufferUnique(staging_info);

        auto mem_req = device.getBufferMemoryRequirements(staging_buffer.get());

        vk::MemoryAllocateInfo alloc_info{
            .allocationSize = mem_req.size,
            .memoryTypeIndex =
                find_memory_type(device_.physical_device(),
                                 mem_req.memoryTypeBits,
                                 vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent)};

        auto staging_memory = device.allocateMemoryUnique(alloc_info);
        device.bindBufferMemory(staging_buffer.get(), staging_memory.get(), 0);

        auto *data = device.mapMemory(staging_memory.get(), 0, mem_req.size);
        std::memcpy(data, image_data.bytes().data(), image_size);
        device.unmapMemory(staging_memory.get());

        vk::ImageCreateInfo image_info{.imageType = vk::ImageType::e2D,
                                       .format = image_format,
                                       .extent = vk::Extent3D{static_cast<std::uint32_t>(image_data.width()),
                                                              static_cast<std::uint32_t>(image_data.height()),
                                                              1},
                                       .mipLevels = 1,
                                       .arrayLayers = 1,
                                       .samples = vk::SampleCountFlagBits::e1,
                                       .tiling = vk::ImageTiling::eOptimal,
                                       .usage = vk::ImageUsageFlagBits::eTransferDst | vk::ImageUsageFlagBits::eSampled,
                                       .sharingMode = vk::SharingMode::eExclusive,
                                       .initialLayout = vk::ImageLayout::eUndefined};

        auto image = device.createImageUnique(image_info);
        auto img_mem_req = device.getImageMemoryRequirements(image.get());

        vk::MemoryAllocateInfo img_alloc_info{.allocationSize = img_mem_req.size,
                                              .memoryTypeIndex =
                                                  find_memory_type(device_.physical_device(),
                                                                   img_mem_req.memoryTypeBits,
                                                                   vk::MemoryPropertyFlagBits::eDeviceLocal)};

        auto img_memory = device.allocateMemoryUnique(img_alloc_info);
        device.bindImageMemory(image.get(), img_memory.get(), 0);

        // Perform the copy + transitions
        {
            vk::UniqueCommandBuffer cmd = begin_one_shot_commands();

            transition_image_layout(cmd.get(),
                                    image.get(),
                                    vk::ImageLayout::eUndefined,
                                    vk::ImageLayout::eTransferDstOptimal);

            vk::BufferImageCopy region{
                .bufferOffset = 0,
                .bufferRowLength = 0,
                .bufferImageHeight = 0,
                .imageSubresource =
                    vk::ImageSubresourceLayers{
                        .aspectMask = vk::ImageAspectFlagBits::eColor,
                        .mipLevel = 0,
                        .baseArrayLayer = 0,
                        .layerCount = 1,
                    },
                .imageOffset = vk::Offset3D{0, 0, 0},
                .imageExtent = vk::Extent3D{static_cast<std::uint32_t>(image_data.width()),
                                            static_cast<std::uint32_t>(image_data.height()),
                                            1},
            };

            cmd->copyBufferToImage(staging_buffer.get(), image.get(), vk::ImageLayout::eTransferDstOptimal, 1, &region);

            transition_image_layout(cmd.get(),
                                    image.get(),
                                    vk::ImageLayout::eTransferDstOptimal,
                                    vk::ImageLayout::eShaderReadOnlyOptimal);

            end_one_shot_commands(std::move(cmd));
        }

        vk::ImageViewCreateInfo view_info{.image = image.get(),
                                          .viewType = vk::ImageViewType::e2D,
                                          .format = image_format,
                                          .components = vk::ComponentMapping{},
                                          .subresourceRange =
                                              vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1}};

        vk::UniqueImageView image_view = device.createImageViewUnique(view_info);

        return std::make_unique<VulkanTextureRenderData>(std::move(image),
                                                         std::move(img_memory),
                                                         std::move(image_view),
                                                         linear_sampler_.get(),
                                                         image_data.width(),
                                                         image_data.height());
    }

    vk::UniqueCommandBuffer VulkanTextureManager::begin_one_shot_commands() const
    {
        const auto device = device_.device();

        const vk::CommandBufferAllocateInfo alloc_info{
            .commandPool = command_pool_,
            .level = vk::CommandBufferLevel::ePrimary,
            .commandBufferCount = 1,
        };

        auto buffers = device.allocateCommandBuffersUnique(alloc_info);
        vk::UniqueCommandBuffer cmd = std::move(buffers.front());

        const vk::CommandBufferBeginInfo begin_info{
            .flags = vk::CommandBufferUsageFlagBits::eOneTimeSubmit,
        };

        cmd->begin(begin_info);
        return cmd;
    }

    void VulkanTextureManager::end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const
    {
        cmd->end();

        auto device = device_.device();

        vk::FenceCreateInfo fence_info{};
        vk::UniqueFence fence = device.createFenceUnique(fence_info);

        vk::CommandBuffer raw_cmd = cmd.get();
        vk::SubmitInfo submit_info{
            .commandBufferCount = 1,
            .pCommandBuffers = &raw_cmd,
        };

        if (device_.graphics_queue().submit(1, &submit_info, fence.get()) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit one-shot command buffer"};
        }

        // Simple and safe for asset loading. If you later want async streaming, swap this for a timeline semaphore.
        if (device.waitForFences(1, &fence.get(), vk::True, std::numeric_limits<std::uint64_t>::max()) !=
            vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed waiting for one-shot fence"};
        }
    }

    void VulkanTextureManager::transition_image_layout(vk::CommandBuffer cmd,
                                                       vk::Image image,
                                                       vk::ImageLayout old_layout,
                                                       vk::ImageLayout new_layout)
    {
        vk::AccessFlags src_access{};
        vk::AccessFlags dst_access{};
        vk::PipelineStageFlags src_stage{};
        vk::PipelineStageFlags dst_stage{};

        if (old_layout == vk::ImageLayout::eUndefined && new_layout == vk::ImageLayout::eTransferDstOptimal)
        {
            src_access = vk::AccessFlagBits::eNone;
            dst_access = vk::AccessFlagBits::eTransferWrite;
            src_stage = vk::PipelineStageFlagBits::eTopOfPipe;
            dst_stage = vk::PipelineStageFlagBits::eTransfer;
        }
        else if (old_layout == vk::ImageLayout::eTransferDstOptimal &&
                 new_layout == vk::ImageLayout::eShaderReadOnlyOptimal)
        {
            src_access = vk::AccessFlagBits::eTransferWrite;
            dst_access = vk::AccessFlagBits::eShaderRead;
            src_stage = vk::PipelineStageFlagBits::eTransfer;
            dst_stage = vk::PipelineStageFlagBits::eFragmentShader;
        }
        else
        {
            throw std::runtime_error{"VulkanRenderer2D: unsupported image layout transition"};
        }

        vk::ImageMemoryBarrier barrier{
            .srcAccessMask = src_access,
            .dstAccessMask = dst_access,
            .oldLayout = old_layout,
            .newLayout = new_layout,
            .srcQueueFamilyIndex = vk::QueueFamilyIgnored,
            .dstQueueFamilyIndex = vk::QueueFamilyIgnored,
            .image = image,
            .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
        };

        cmd.pipelineBarrier(src_stage, dst_stage, {}, 0, nullptr, 0, nullptr, 1, &barrier);
    }
} // namespace retro
