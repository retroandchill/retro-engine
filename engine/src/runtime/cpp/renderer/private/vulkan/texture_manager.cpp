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
import retro.runtime.exceptions;

namespace retro
{
    VulkanTextureManager::VulkanTextureManager(VulkanDevice &device)
        : device_{device}, linear_sampler_{device_.create_linear_sampler()}
    {
    }

    std::unique_ptr<TextureRenderData> VulkanTextureManager::upload_texture(const ImageData &image_data)
    {
        auto command_pool = get_thread_command_pool();

        auto image_size = image_data.bytes().size();
        auto image_format = vk::Format::eR8G8B8A8Srgb;

        vk::BufferCreateInfo staging_info{.size = image_size,
                                          .usage = vk::BufferUsageFlagBits::eTransferSrc,
                                          .sharingMode = vk::SharingMode::eExclusive};

        auto staging_buffer = device_.create_staging_buffer(image_size);
        staging_buffer.copy_to_buffer(image_data.bytes());

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

        auto image = device_.create_image(image_info);
        auto img_memory = device_.allocate_image_memory(image.get());

        // Perform the copy + transitions
        {
            vk::UniqueCommandBuffer cmd = begin_one_shot_commands(command_pool);

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

        vk::UniqueImageView image_view = device_.create_image_view(view_info);

        return std::make_unique<VulkanTextureRenderData>(std::move(image),
                                                         std::move(img_memory),
                                                         std::move(image_view),
                                                         linear_sampler_.get(),
                                                         image_data.width(),
                                                         image_data.height());
    }

    vk::CommandPool VulkanTextureManager::get_thread_command_pool() const
    {
        auto thread_id = std::this_thread::get_id();

        {
            std::shared_lock lock{thread_pools_mutex_};

            auto it = thread_pools_.find(thread_id);
            if (it != thread_pools_.end())
            {
                return it->second.get();
            }
        }

        std::unique_lock lock{thread_pools_mutex_};

        auto &pool = thread_pools_[thread_id] = device_.create_command_pool();
        return pool.get();
    }

    vk::UniqueCommandBuffer VulkanTextureManager::begin_one_shot_commands(vk::CommandPool command_pool) const
    {
        const vk::CommandBufferAllocateInfo alloc_info{
            .commandPool = command_pool,
            .level = vk::CommandBufferLevel::ePrimary,
            .commandBufferCount = 1,
        };

        auto buffers = device_.create_command_buffers(alloc_info);
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

        auto fence = device_.create_fence();

        auto raw_cmd = cmd.get();
        vk::SubmitInfo submit_info{
            .commandBufferCount = 1,
            .pCommandBuffers = &raw_cmd,
        };

        device_.submit_to_graphics_queue(
            [&submit_info, &fence](vk::Queue graphics_queue)
            {
                if (graphics_queue.submit(1, &submit_info, fence.get()) != vk::Result::eSuccess)
                {
                    throw GraphicsException{"VulkanRenderer2D: failed to submit one-shot command buffer"};
                }
            });

        device_.wait_for_fences(fence.get());
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
            throw GraphicsException{"VulkanRenderer2D: unsupported image layout transition"};
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
