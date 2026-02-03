/**
 * @file renderer.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

#include <cassert>
#include <SDL3/SDL_vulkan.h>

module retro.renderer;

import retro.logging;
import vulkan_hpp;

namespace retro
{
    namespace
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
    } // namespace

    std::unique_ptr<VulkanBufferManager> VulkanBufferManager::instance_{nullptr};

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

    void VulkanBufferManager::initialize(const VulkanDevice &device, const std::size_t pool_size)
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

    VulkanRenderer2D::VulkanRenderer2D(std::shared_ptr<Window> viewport)
        : viewport_{std::move(viewport)}, instance_{create_instance(*viewport_)},
          surface_{create_surface(*viewport_, instance_.get())}, device_{instance_.get(), surface_.get()},
          buffer_manager_{device_}, swapchain_(SwapchainConfig{
                                        .physical_device = device_.physical_device(),
                                        .device = device_.device(),
                                        .surface = surface_.get(),
                                        .graphics_family = device_.graphics_family_index(),
                                        .present_family = device_.present_family_index(),
                                        .width = viewport_->width(),
                                        .height = viewport_->height(),
                                    }),
          render_pass_(create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1)),
          framebuffers_(create_framebuffers(device_.device(), render_pass_.get(), swapchain_)),
          command_pool_(CommandPoolConfig{
              .device = device_.device(),
              .queue_family_idx = device_.graphics_family_index(),
              .buffer_count = MAX_FRAMES_IN_FLIGHT,
          }),
          sync_(SyncConfig{
              .device = device_.device(),
              .frames_in_flight = MAX_FRAMES_IN_FLIGHT,
              .swapchain_image_count = static_cast<std::uint32_t>(swapchain_.image_views().size()),
          }),
          pipeline_manager_{device_.device()}, linear_sampler_{create_linear_sampler()}
    {
    }

    void VulkanRenderer2D::wait_idle()
    {
        if (device_.device() != nullptr)
        {
            device_.device().waitIdle();
        }
    }

    void VulkanRenderer2D::begin_frame()
    {
        auto dev = device_.device();

        auto in_flight = sync_.in_flight(current_frame_);
        if (dev.waitForFences(1, &in_flight, vk::True, std::numeric_limits<std::uint64_t>::max()) ==
            vk::Result::eTimeout)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to wait for fence"};
        }

        dev.resetDescriptorPool(sync_.descriptor_pool(current_frame_));

        auto result = dev.acquireNextImageKHR(swapchain_.handle(),
                                              std::numeric_limits<std::uint64_t>::max(),
                                              sync_.image_available(current_frame_),
                                              nullptr,
                                              &image_index_);

        if (result == vk::Result::eErrorOutOfDateKHR)
        {
            recreate_swapchain();
            return;
        }

        if (result != vk::Result::eSuccess && result != vk::Result::eSuboptimalKHR)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to acquire swapchain image"};
        }

        dev.resetFences({in_flight});
    }

    void VulkanRenderer2D::end_frame()
    {
        const auto in_flight = sync_.in_flight(current_frame_);
        auto cmd = command_pool_.buffer_at(current_frame_);

        cmd.reset();
        record_command_buffer(cmd, image_index_);

        std::array wait_semaphores = {sync_.image_available(current_frame_)};
        std::array wait_stages = {
            static_cast<vk::PipelineStageFlags>(vk::PipelineStageFlagBits::eColorAttachmentOutput)};

        // Signal semaphore is now per-image
        const vk::Semaphore render_finished_semaphore = sync_.render_finished(image_index_);
        std::array signal_semaphores = {render_finished_semaphore};

        const vk::SubmitInfo submit_info{.waitSemaphoreCount = wait_semaphores.size(),
                                         .pWaitSemaphores = wait_semaphores.data(),
                                         .pWaitDstStageMask = wait_stages.data(),
                                         .commandBufferCount = 1,
                                         .pCommandBuffers = &cmd,
                                         .signalSemaphoreCount = signal_semaphores.size(),
                                         .pSignalSemaphores = signal_semaphores.data()};

        if (device_.graphics_queue().submit(1, &submit_info, in_flight) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
        }

        std::array swapchains = {swapchain_.handle()};

        vk::PresentInfoKHR present_info{.waitSemaphoreCount = signal_semaphores.size(),
                                        .pWaitSemaphores = signal_semaphores.data(),
                                        .swapchainCount = swapchains.size(),
                                        .pSwapchains = swapchains.data(),
                                        .pImageIndices = &image_index_};

        if (const auto result = device_.present_queue().presentKHR(&present_info);
            result == vk::Result::eErrorOutOfDateKHR || result == vk::Result::eSuboptimalKHR)
        {
            recreate_swapchain();
        }
        else if (result != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to present swapchain image"};
        }

        current_frame_ = (current_frame_ + 1) % MAX_FRAMES_IN_FLIGHT;
        pipeline_manager_.clear_draw_queue();
        VulkanBufferManager::instance().reset();
    }

    Vector2u VulkanRenderer2D::viewport_size() const
    {
        return viewport_->size();
    }

    void VulkanRenderer2D::add_new_render_pipeline(const std::type_index type, std::shared_ptr<RenderPipeline> pipeline)
    {
        pipeline_manager_.create_pipeline(type, std::move(pipeline), swapchain_, render_pass_.get());
    }

    void VulkanRenderer2D::remove_render_pipeline(const std::type_index type)
    {
        pipeline_manager_.destroy_pipeline(type);
    }

    std::unique_ptr<TextureRenderData> VulkanRenderer2D::upload_texture(const ImageData &image_data)
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

    vk::UniqueInstance VulkanRenderer2D::create_instance(const Window &viewport)
    {
        vk::ApplicationInfo app_info{.pApplicationName = "Retro Engine",
                                     .applicationVersion = vk::makeVersion(1, 0, 0),
                                     .pEngineName = "Retro Engine",
                                     .engineVersion = vk::makeVersion(1, 0, 0),
                                     .apiVersion = vk::makeApiVersion(0, 1, 2, 0)};

        std::vector<const char *> enabled_layers;
#ifndef NDEBUG
        auto available_layers = vk::enumerateInstanceLayerProperties();
        const bool has_validation =
            std::ranges::any_of(available_layers,
                                [](const vk::LayerProperties &lp)
                                { return std::string_view{lp.layerName} == "VK_LAYER_KHRONOS_validation"; });

        if (has_validation)
        {
            enabled_layers.push_back("VK_LAYER_KHRONOS_validation");
        }
        else
        {
            get_logger().warn("Vulkan validation layers requested, but not available!");
        }
#endif

        const auto extensions = get_required_instance_extensions(viewport);

        std::vector validation_feature_enables = {vk::ValidationFeatureEnableEXT::eDebugPrintf};

        vk::ValidationFeaturesEXT validation_features{.enabledValidationFeatureCount =
                                                          static_cast<std::uint32_t>(validation_feature_enables.size()),
                                                      .pEnabledValidationFeatures = validation_feature_enables.data()};

        const vk::InstanceCreateInfo create_info{.pNext = &validation_features,
                                                 .pApplicationInfo = &app_info,
                                                 .enabledLayerCount = static_cast<std::uint32_t>(enabled_layers.size()),
                                                 .ppEnabledLayerNames = enabled_layers.data(),
                                                 .enabledExtensionCount = static_cast<std::uint32_t>(extensions.size()),
                                                 .ppEnabledExtensionNames = extensions.data()};

        return vk::createInstanceUnique(create_info);
    }

    vk::UniqueSurfaceKHR VulkanRenderer2D::create_surface(const Window &viewport, vk::Instance instance)
    {
        switch (auto [backend, handle] = viewport.native_handle(); backend)
        {
            case WindowBackend::SDL3:
                {
                    vk::SurfaceKHR::CType surface;
                    if (!SDL_Vulkan_CreateSurface(static_cast<SDL_Window *>(handle), instance, nullptr, &surface))
                    {
                        throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
                    }

                    return vk::UniqueSurfaceKHR{surface, instance};
                }
        }

        throw std::runtime_error{"Unsupported window backend"};
    }

    std::span<const char *const> VulkanRenderer2D::get_required_instance_extensions(const Window &viewport)
    {
        auto [backend, handle] = viewport.native_handle();
        switch (backend)
        {
            case WindowBackend::SDL3:
                {
                    std::uint32_t count = 0;
                    auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
                    if (names == nullptr)
                    {
                        throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
                    }

                    return std::span{names, count};
                }
        }

        get_logger().error("Unsupported window backend:");
        return {};
    }

    vk::UniqueRenderPass VulkanRenderer2D::create_render_pass(vk::Device device,
                                                              vk::Format color_format,
                                                              vk::SampleCountFlagBits samples)
    {
        vk::AttachmentDescription color_attachment{{},
                                                   color_format,
                                                   samples,
                                                   vk::AttachmentLoadOp::eClear,
                                                   vk::AttachmentStoreOp::eStore,
                                                   vk::AttachmentLoadOp::eDontCare,
                                                   vk::AttachmentStoreOp::eDontCare,
                                                   vk::ImageLayout::eUndefined,
                                                   vk::ImageLayout::ePresentSrcKHR};

        vk::AttachmentReference color_ref{0, vk::ImageLayout::eColorAttachmentOptimal};

        vk::SubpassDescription subpass{{}, vk::PipelineBindPoint::eGraphics, 0, nullptr, 1, &color_ref};

        vk::SubpassDependency dependency{vk::SubpassExternal,
                                         0,
                                         vk::PipelineStageFlagBits::eColorAttachmentOutput,
                                         vk::PipelineStageFlagBits::eColorAttachmentOutput,
                                         vk::AccessFlagBits::eNone,
                                         vk::AccessFlagBits::eColorAttachmentWrite,
                                         vk::DependencyFlagBits::eByRegion};

        vk::RenderPassCreateInfo rp_info{.attachmentCount = 1,
                                         .pAttachments = &color_attachment,
                                         .subpassCount = 1,
                                         .pSubpasses = &subpass,
                                         .dependencyCount = 1,
                                         .pDependencies = &dependency};

        return device.createRenderPassUnique(rp_info);
    }

    std::vector<vk::UniqueFramebuffer> VulkanRenderer2D::create_framebuffers(vk::Device device,
                                                                             vk::RenderPass render_pass,
                                                                             const VulkanSwapchain &swapchain)
    {
        return swapchain.image_views() |
               std::views::transform(
                   [device, render_pass, &swapchain](const vk::UniqueImageView &image)
                   {
                       std::array attachments = {image.get()};

                       vk::FramebufferCreateInfo fb_info{.renderPass = render_pass,
                                                         .attachmentCount = attachments.size(),
                                                         .pAttachments = attachments.data(),
                                                         .width = swapchain.extent().width,
                                                         .height = swapchain.extent().height,
                                                         .layers = 1};

                       return device.createFramebufferUnique(fb_info);
                   }) |
               std::ranges::to<std::vector>();
    }

    vk::UniqueSampler VulkanRenderer2D::create_linear_sampler() const
    {
        const vk::SamplerCreateInfo sampler_info{
            .magFilter = vk::Filter::eLinear,
            .minFilter = vk::Filter::eLinear,
            .mipmapMode = vk::SamplerMipmapMode::eLinear,
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

        return device_.device().createSamplerUnique(sampler_info);
    }

    void VulkanRenderer2D::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = viewport_->size();
        if (w == 0 || h == 0)
            return;

        device_.device().waitIdle();

        swapchain_ = VulkanSwapchain{SwapchainConfig{SwapchainConfig{.physical_device = device_.physical_device(),
                                                                     .device = device_.device(),
                                                                     .surface = surface_.get(),
                                                                     .graphics_family = device_.graphics_family_index(),
                                                                     .present_family = device_.present_family_index(),
                                                                     .width = w,
                                                                     .height = h,
                                                                     .old_swapchain = swapchain_.handle()}}};
        render_pass_ = create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1);
        framebuffers_ = create_framebuffers(device_.device(), render_pass_.get(), swapchain_);
        pipeline_manager_.recreate_pipelines(swapchain_, render_pass_.get());
    }

    void VulkanRenderer2D::record_command_buffer(const vk::CommandBuffer cmd, const std::uint32_t image_index)
    {
        constexpr vk::CommandBufferBeginInfo begin_info{};

        cmd.begin(begin_info);
        vk::ClearValue clear{.color = vk::ClearColorValue{.float32 = std::array{0.0f, 0.0f, 0.0f, 1.0f}}};

        const vk::RenderPassBeginInfo rp_info{.renderPass = render_pass_.get(),
                                              .framebuffer = framebuffers_.at(image_index).get(),
                                              .renderArea = vk::Rect2D{vk::Offset2D{0, 0}, swapchain_.extent()},
                                              .clearValueCount = 1,
                                              .pClearValues = &clear};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);
        pipeline_manager_.bind_and_render(cmd, viewport_->size(), sync_.descriptor_pool(current_frame_));
        cmd.endRenderPass();
        cmd.end();
    }

    vk::UniqueCommandBuffer VulkanRenderer2D::begin_one_shot_commands() const
    {
        const auto device = device_.device();

        const vk::CommandBufferAllocateInfo alloc_info{
            .commandPool = command_pool_.pool(),
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

    void VulkanRenderer2D::end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const
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

    void VulkanRenderer2D::transition_image_layout(vk::CommandBuffer cmd,
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
