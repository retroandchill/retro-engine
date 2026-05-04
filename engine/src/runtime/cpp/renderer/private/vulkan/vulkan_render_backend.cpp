/**
 * @file vulkan_render_backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.vulkan_render_backend;
import retro.renderer.vulkan.renderer;
import retro.runtime.exceptions;

namespace retro
{

    VulkanRenderBackend::VulkanRenderBackend(PlatformBackend &platform_backend)
        : instance_{platform_backend.window_backend()}, device_{instance_, platform_backend},
          buffer_manager_{device_.create_buffer_manager()}, command_pool_{device_.create_command_pool()},
          linear_sampler_{device_.create_linear_sampler()}
    {
    }

    std::shared_ptr<Renderer2D> VulkanRenderBackend::create_renderer(std::shared_ptr<Window> window)
    {
        auto surface = instance_.create_surface(*window);
        return std::make_shared<VulkanRenderer2D>(*this,
                                                  std::move(window),
                                                  std::move(surface),
                                                  device_,
                                                  buffer_manager_,
                                                  command_pool_.get());
    }

    RefCountPtr<Texture> VulkanRenderBackend::upload_texture(const std::span<const std::byte> bytes,
                                                             const std::int32_t width,
                                                             const std::int32_t height,
                                                             const TextureFormat format)
    {
        auto command_pool = get_thread_command_pool();

        auto image_size = bytes.size();
        auto image_format = vk::Format::eUndefined;
        switch (format)
        {
            case TextureFormat::rgba8:
                image_format = vk::Format::eR8G8B8A8Srgb;
                break;
            case TextureFormat::rgba16f:
                image_format = vk::Format::eR16G16B16A16Sfloat;
                break;
            default:
                throw GraphicsException{"VulkanRenderer2D: unsupported texture format"};
        }

        auto staging_buffer = device_.create_staging_buffer(image_size);
        staging_buffer.read_from_buffer(bytes);

        vk::ImageCreateInfo image_info{
            .imageType = vk::ImageType::e2D,
            .format = image_format,
            .extent = vk::Extent3D{static_cast<std::uint32_t>(width), static_cast<std::uint32_t>(height), 1},
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
            auto cmd = begin_one_shot_commands(command_pool);

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
                .imageExtent = vk::Extent3D{static_cast<std::uint32_t>(width), static_cast<std::uint32_t>(height), 1},
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

        return make_ref_counted<VulkanTexture>(shared_from_this(),
                                               std::move(image),
                                               std::move(img_memory),
                                               std::move(image_view),
                                               linear_sampler_.get(),
                                               width,
                                               height,
                                               format);
    }

    std::pair<bool, std::size_t> VulkanRenderBackend::export_texture(const Texture &texture,
                                                                     std::span<std::byte> buffer)
    {
        auto &vulkan_texture = dynamic_cast<const VulkanTexture &>(texture);
        std::size_t expected_size = 0;
        switch (vulkan_texture.format())
        {
            case TextureFormat::rgba8:
                expected_size = vulkan_texture.width() * vulkan_texture.height() * 4;
                break;
            case TextureFormat::rgba16f:
                expected_size = vulkan_texture.width() * vulkan_texture.height() * 8;
                break;
        }

        if (buffer.size() < expected_size)
        {
            return {false, 0};
        }

        const auto command_pool = get_thread_command_pool();
        auto cmd = begin_one_shot_commands(command_pool);
        transition_image_layout(cmd.get(),
                                vulkan_texture.image(),
                                vk::ImageLayout::eShaderReadOnlyOptimal,
                                vk::ImageLayout::eTransferSrcOptimal);
        constexpr vk::BufferImageCopy region{.bufferOffset = 0,
                                             .bufferRowLength = 0,
                                             .bufferImageHeight = 0,
                                             .imageSubresource = vk::ImageSubresourceLayers{
                                                 .aspectMask = vk::ImageAspectFlagBits::eColor,
                                                 .mipLevel = 0,
                                                 .baseArrayLayer = 0,
                                                 .layerCount = 1,
                                             }};

        const auto staging_buffer = device_.create_staging_buffer(buffer.size());
        cmd->copyImageToBuffer(vulkan_texture.image(),
                               vk::ImageLayout::eTransferSrcOptimal,
                               staging_buffer.get(),
                               region);
        const auto result = staging_buffer.write_to_buffer(buffer);
        transition_image_layout(cmd.get(),
                                vulkan_texture.image(),
                                vk::ImageLayout::eTransferSrcOptimal,
                                vk::ImageLayout::eShaderReadOnlyOptimal);
        end_one_shot_commands(std::move(cmd));
        return result;
    }

    vk::CommandPool VulkanRenderBackend::get_thread_command_pool() const
    {
        const auto thread_id = std::this_thread::get_id();

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

    vk::UniqueCommandBuffer VulkanRenderBackend::begin_one_shot_commands(vk::CommandPool command_pool) const
    {
        const vk::CommandBufferAllocateInfo alloc_info{
            .commandPool = command_pool,
            .level = vk::CommandBufferLevel::ePrimary,
            .commandBufferCount = 1,
        };

        auto buffers = device_.create_command_buffers(alloc_info);
        vk::UniqueCommandBuffer cmd = std::move(buffers.front());

        constexpr vk::CommandBufferBeginInfo begin_info{
            .flags = vk::CommandBufferUsageFlagBits::eOneTimeSubmit,
        };

        cmd->begin(begin_info);
        return cmd;
    }

    void VulkanRenderBackend::end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const
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
    void VulkanRenderBackend::transition_image_layout(vk::CommandBuffer cmd,
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
        else if (old_layout == vk::ImageLayout::eShaderReadOnlyOptimal &&
                 new_layout == vk::ImageLayout::eTransferSrcOptimal)
        {
            src_access = vk::AccessFlagBits::eShaderRead;
            dst_access = vk::AccessFlagBits::eTransferRead;
            src_stage = vk::PipelineStageFlagBits::eFragmentShader;
            dst_stage = vk::PipelineStageFlagBits::eTransfer;
        }
        else if (old_layout == vk::ImageLayout::eTransferSrcOptimal &&
                 new_layout == vk::ImageLayout::eShaderReadOnlyOptimal)
        {
            src_access = vk::AccessFlagBits::eTransferRead;
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
