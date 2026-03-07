/**
 * @file presenter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

#include <cassert>

module retro.renderer.vulkan.components.presenter;

import retro.runtime.exceptions;

namespace retro
{

    namespace
    {
        vk::UniqueRenderPass create_render_pass(VulkanDevice &device,
                                                const vk::Format color_format,
                                                const vk::SampleCountFlagBits samples)
        {
            const vk::AttachmentDescription color_attachment{.format = color_format,
                                                             .samples = samples,
                                                             .loadOp = vk::AttachmentLoadOp::eClear,
                                                             .storeOp = vk::AttachmentStoreOp::eStore,
                                                             .stencilLoadOp = vk::AttachmentLoadOp::eDontCare,
                                                             .stencilStoreOp = vk::AttachmentStoreOp::eDontCare,
                                                             .initialLayout = vk::ImageLayout::eUndefined,
                                                             .finalLayout = vk::ImageLayout::ePresentSrcKHR};

            const vk::AttachmentDescription depth_attachment{.format = vk::Format::eD32Sfloat,
                                                             .samples = samples,
                                                             .loadOp = vk::AttachmentLoadOp::eClear,
                                                             .storeOp = vk::AttachmentStoreOp::eDontCare,
                                                             .stencilLoadOp = vk::AttachmentLoadOp::eDontCare,
                                                             .stencilStoreOp = vk::AttachmentStoreOp::eDontCare,
                                                             .initialLayout = vk::ImageLayout::eUndefined,
                                                             .finalLayout =
                                                                 vk::ImageLayout::eDepthStencilAttachmentOptimal};

            std::array attachments = {color_attachment, depth_attachment};

            vk::AttachmentReference color_ref{.attachment = 0, .layout = vk::ImageLayout::eColorAttachmentOptimal};
            vk::AttachmentReference depth_ref{.attachment = 1,
                                              .layout = vk::ImageLayout::eDepthStencilAttachmentOptimal};

            vk::SubpassDescription subpass{.pipelineBindPoint = vk::PipelineBindPoint::eGraphics,
                                           .colorAttachmentCount = 1,
                                           .pColorAttachments = &color_ref,
                                           .pDepthStencilAttachment = &depth_ref};

            vk::SubpassDependency dependency{.srcSubpass = vk::SubpassExternal,
                                             .srcStageMask = vk::PipelineStageFlagBits::eColorAttachmentOutput |
                                                             vk::PipelineStageFlagBits::eEarlyFragmentTests,
                                             .dstStageMask = vk::PipelineStageFlagBits::eColorAttachmentOutput |
                                                             vk::PipelineStageFlagBits::eEarlyFragmentTests,
                                             .srcAccessMask = vk::AccessFlagBits::eNone,
                                             .dstAccessMask = vk::AccessFlagBits::eColorAttachmentWrite |
                                                              vk::AccessFlagBits::eDepthStencilAttachmentWrite,
                                             .dependencyFlags = vk::DependencyFlagBits::eByRegion};

            vk::RenderPassCreateInfo rp_info{.attachmentCount = attachments.size(),
                                             .pAttachments = attachments.data(),
                                             .subpassCount = 1,
                                             .pSubpasses = &subpass,
                                             .dependencyCount = 1,
                                             .pDependencies = &dependency};

            return device.create_render_pass(rp_info);
        }
    } // namespace

    void PendingFrameSlot::reset() noexcept
    {
        memory_resource.reset();
        pending_commands.clear();
        pending_commands.shrink_to_fit();
    }

    VulkanPresenter::VulkanPresenter(Window &window,
                                     const vk::SurfaceKHR surface,
                                     VulkanDevice &device,
                                     const vk::CommandPool command_pool,
                                     VulkanPipelineManager &pipeline_manager)
        : window_{window}, surface_{surface}, device_{device}, command_pool_{command_pool},
          pipeline_manager_{pipeline_manager}
    {
        auto [width, height] = window.size();
        create_swapchain(width, height);

        auto command_buffers =
            device_.create_command_buffers(vk::CommandBufferAllocateInfo{.commandPool = command_pool,
                                                                         .level = vk::CommandBufferLevel::ePrimary,
                                                                         .commandBufferCount = max_frames_in_flight});

        constexpr vk::FenceCreateInfo fence_info{.flags = vk::FenceCreateFlagBits::eSignaled};

        std::array pool_sizes = {
            vk::DescriptorPoolSize{vk::DescriptorType::eStorageBuffer, 256},
            vk::DescriptorPoolSize{vk::DescriptorType::eCombinedImageSampler, 256},
        };
        const vk::DescriptorPoolCreateInfo pool_info{.maxSets = 256,
                                                     .poolSizeCount = pool_sizes.size(),
                                                     .pPoolSizes = pool_sizes.data()};

        assert(command_buffers.size() == frame_resources_.size());
        for (auto [index, element] : frame_resources_ | std::views::enumerate)
        {
            element.command_buffer = std::move(command_buffers[index]);
            element.image_available = device_.create_semaphore();
            element.in_flight = device_.create_fence(fence_info);
            element.descriptor_pool = device_.create_descriptor_pool(pool_info);
        }
    }

    void VulkanPresenter::wait_for_current_frame()
    {
        const auto fence = frame_resources_.at(current_frame_).in_flight.get();
        device_.wait_for_fences(fence);
    }

    void VulkanPresenter::queue_frame_for_render(RenderQueueFn factory)
    {
        pending_frame_slots_.produce(
            [factory](PendingFrameSlot &slot)
            {
                slot.pending_commands = factory(slot.memory_resource);
                std::ranges::sort(slot.pending_commands,
                                  [](const DrawCommandSet &lhs, const DrawCommandSet &rhs)
                                  { return lhs.z_order < rhs.z_order; });
            });
    }

    void VulkanPresenter::begin_frame()
    {
        current_frame_ = (current_frame_ + 1) % max_frames_in_flight;
        const auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
        device_.wait_for_fences(in_flight);
        Deferred fence_reset{[this, in_flight]
                             {
                                 device_.reset_fences(in_flight);
                             }};

        device_.reset_descriptor_pool(frame_resources_.at(current_frame_).descriptor_pool.get());

        const auto result = device_.acquire_next_image(swapchain_.get(),
                                                       std::numeric_limits<std::uint64_t>::max(),
                                                       frame_resources_.at(current_frame_).image_available.get(),
                                                       nullptr,
                                                       image_index_);

        if (result == vk::Result::eErrorOutOfDateKHR)
        {
            recreate_swapchain();
            return;
        }

        if (result != vk::Result::eSuccess && result != vk::Result::eSuboptimalKHR)
        {
            throw GraphicsException{"VulkanRenderer2D: failed to acquire swapchain image"};
        }
    }
    void VulkanPresenter::submit_and_present()
    {
        const auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
        auto cmd = frame_resources_.at(current_frame_).command_buffer.get();
        cmd.reset();
        record_command_buffer(cmd);

        std::array wait_semaphores = {frame_resources_.at(current_frame_).image_available.get()};
        std::array wait_stages = {
            static_cast<vk::PipelineStageFlags>(vk::PipelineStageFlagBits::eColorAttachmentOutput)};

        // Signal semaphore is now per-image
        const vk::Semaphore render_finished_semaphore = image_resources_[image_index_].render_finished.get();
        std::array signal_semaphores = {render_finished_semaphore};

        const vk::SubmitInfo submit_info{.waitSemaphoreCount = wait_semaphores.size(),
                                         .pWaitSemaphores = wait_semaphores.data(),
                                         .pWaitDstStageMask = wait_stages.data(),
                                         .commandBufferCount = 1,
                                         .pCommandBuffers = &cmd,
                                         .signalSemaphoreCount = signal_semaphores.size(),
                                         .pSignalSemaphores = signal_semaphores.data()};

        device_.submit_to_graphics_queue(
            [&submit_info, in_flight](vk::Queue graphics_queue)
            {
                if (graphics_queue.submit(1, &submit_info, in_flight) != vk::Result::eSuccess)
                {
                    throw GraphicsException{"VulkanRenderer2D: failed to submit draw command buffer"};
                }
            });

        std::array swapchains = {swapchain_.get()};

        vk::PresentInfoKHR present_info{.waitSemaphoreCount = signal_semaphores.size(),
                                        .pWaitSemaphores = signal_semaphores.data(),
                                        .swapchainCount = swapchains.size(),
                                        .pSwapchains = swapchains.data(),
                                        .pImageIndices = &image_index_};

        device_.submit_to_present_queue(
            [this, &present_info](vk::Queue present_queue)
            {
                if (const auto result = present_queue.presentKHR(&present_info);
                    result == vk::Result::eErrorOutOfDateKHR || result == vk::Result::eSuboptimalKHR)
                {
                    recreate_swapchain();
                }
                else if (result != vk::Result::eSuccess)
                {
                    throw GraphicsException{"VulkanRenderer2D: failed to present swapchain image"};
                }
            });
    }

    void VulkanPresenter::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {

        pipeline_manager_.create_pipeline(type, pipeline, extent_, render_pass_.get());
    }

    void VulkanPresenter::remove_render_pipeline(const std::type_index type) const
    {
        pipeline_manager_.destroy_pipeline(type);
    }

    void VulkanPresenter::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = window_.size();
        if (w == 0 || h == 0)
            return;

        device_.wait_idle();
        create_swapchain(w, h);
        pipeline_manager_.recreate_pipelines(extent_, render_pass_.get());
    }
    void VulkanPresenter::record_command_buffer(vk::CommandBuffer cmd)
    {
        constexpr vk::CommandBufferBeginInfo begin_info{};

        cmd.begin(begin_info);
        Deferred end_cmd{[cmd]
                         {
                             cmd.end();
                         }};

        // ReSharper disable once CppDFAUnusedValue
        // ReSharper disable once CppDFAUnreadVariable
        vk::ClearValue color_clear_value{.color = vk::ClearColorValue{.float32 = std::array{0.0f, 0.0f, 0.0f, 1.0f}}};
        vk::ClearValue depth_clear_value{.depthStencil = vk::ClearDepthStencilValue{.depth = 1.0f}};

        std::array clear_values = {color_clear_value, depth_clear_value};

        auto [screen_width, screen_height] = extent_;

        const vk::RenderPassBeginInfo rp_info{
            .renderPass = render_pass_.get(),
            .framebuffer = image_resources_[image_index_].framebuffer.get(),
            .renderArea = vk::Rect2D{.offset = vk::Offset2D{.x = 0, .y = 0},
                                     .extent = vk::Extent2D{.width = screen_width, .height = screen_height}},
            .clearValueCount = clear_values.size(),
            .pClearValues = clear_values.data()};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);
        Deferred end_rp{[cmd]
                        {
                            cmd.endRenderPass();
                        }};

        pending_frame_slots_.consume(
            [this, cmd, screen_width, screen_height](PendingFrameSlot &slot)
            {
                Vector2u framebuffer_size{screen_width, screen_height};
                auto descriptor_pool = frame_resources_.at(current_frame_).descriptor_pool.get();
                for (auto &draw_command : slot.pending_commands)
                {
                    auto [x, y, width, height] = draw_command.layout.to_screen_rect(framebuffer_size);

                    // Set viewport and scissor for this viewport only
                    const vk::Viewport vp{.x = static_cast<float>(x),
                                          .y = static_cast<float>(y),
                                          .width = static_cast<float>(width),
                                          .height = static_cast<float>(height),
                                          .minDepth = 0.0f,
                                          .maxDepth = 1.0f};

                    const vk::Rect2D scissor{.offset = vk::Offset2D{.x = x, .y = y},
                                             .extent = vk::Extent2D{.width = width, .height = height}};

                    cmd.setViewport(0, vp);
                    cmd.setScissor(0, scissor);

                    pipeline_manager_.bind_and_render(cmd, framebuffer_size, draw_command.sources, descriptor_pool);
                }
            });
    }

    void VulkanPresenter::create_swapchain(std::uint32_t width, std::uint32_t height)
    {
        const auto capabilities = device_.get_surface_capabilities(surface_);

        const vk::Extent2D desired_extent{width, height};

        vk::Extent2D actual_extent{std::clamp<std::uint32_t>(desired_extent.width,
                                                             capabilities.minImageExtent.width,
                                                             capabilities.maxImageExtent.width),
                                   std::clamp<std::uint32_t>(desired_extent.height,
                                                             capabilities.minImageExtent.height,
                                                             capabilities.maxImageExtent.height)};

        const auto formats = device_.get_surface_formats(surface_);
        if (formats.empty())
        {
            throw GraphicsException{"VulkanSwapchain: no surface formats"};
        }

        auto chosen_format = formats[0];
        for (const auto &f : formats)
        {
            if (f.format == vk::Format::eB8G8R8A8Srgb && f.colorSpace == vk::ColorSpaceKHR::eSrgbNonlinear)
            {
                chosen_format = f;
                break;
            }
        }

        if (const auto present_modes = device_.get_surface_preset_modes(surface_); present_modes.empty())
        {
            throw GraphicsException{"VulkanSwapchain: no present modes"};
        }

        auto chosen_present_mode = vk::PresentModeKHR::eFifo; // always available

        std::uint32_t image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        vk::SwapchainCreateInfoKHR ci{
            .surface = surface_,
            .minImageCount = image_count,
            .imageFormat = chosen_format.format,
            .imageColorSpace = chosen_format.colorSpace,
            .imageExtent = actual_extent,
            .imageArrayLayers = 1,
            .imageUsage = vk::ImageUsageFlagBits::eColorAttachment,
        };

        const auto graphics_family = device_.graphics_family();
        const auto present_family = device_.present_family();
        const std::array queue_family_indices = {graphics_family, present_family};

        if (graphics_family != present_family)
        {
            ci.imageSharingMode = vk::SharingMode::eConcurrent;
            ci.queueFamilyIndexCount = queue_family_indices.size();
            ci.pQueueFamilyIndices = queue_family_indices.data();
        }
        else
        {
            ci.imageSharingMode = vk::SharingMode::eExclusive;
        }

        ci.preTransform = capabilities.currentTransform;
        ci.compositeAlpha = vk::CompositeAlphaFlagBitsKHR::eOpaque;
        ci.presentMode = chosen_present_mode;
        ci.clipped = vk::True;
        ci.oldSwapchain = swapchain_.get();

        swapchain_ = device_.create_swapchain(ci);

        format_ = chosen_format.format;
        extent_ = actual_extent;

        render_pass_ = create_render_pass(device_, format_, vk::SampleCountFlagBits::e1);
        image_resources_ =
            device_.get_swapchain_images(swapchain_.get()) |
            std::views::transform(
                [this, &actual_extent](const vk::Image image)
                {
                    auto depth_image = device_.create_image(
                        vk::ImageCreateInfo{.imageType = vk::ImageType::e2D,
                                            .format = vk::Format::eD32Sfloat,
                                            .extent = {actual_extent.width, actual_extent.height, 1},
                                            .mipLevels = 1,
                                            .arrayLayers = 1,
                                            .samples = vk::SampleCountFlagBits::e1,
                                            .tiling = vk::ImageTiling::eOptimal,
                                            .usage = vk::ImageUsageFlagBits::eDepthStencilAttachment,
                                            .sharingMode = vk::SharingMode::eExclusive,
                                            .initialLayout = vk::ImageLayout::eUndefined});

                    auto depth_memory = device_.allocate_image_memory(depth_image.get());

                    auto color_view = device_.create_image_view(vk::ImageViewCreateInfo{
                        .image = image,
                        .viewType = vk::ImageViewType::e2D,
                        .format = format_,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
                    });

                    auto depth_view = device_.create_image_view(vk::ImageViewCreateInfo{
                        .image = depth_image.get(),
                        .viewType = vk::ImageViewType::e2D,
                        .format = vk::Format::eD32Sfloat,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eDepth, 0, 1, 0, 1},
                    });

                    std::array attachments = {color_view.get(), depth_view.get()};

                    const vk::FramebufferCreateInfo fb_info{.renderPass = render_pass_.get(),
                                                            .attachmentCount = attachments.size(),
                                                            .pAttachments = attachments.data(),
                                                            .width = extent_.width,
                                                            .height = extent_.height,
                                                            .layers = 1};

                    return VulkanImageResources{.color_image = image,
                                                .color_image_view = std::move(color_view),
                                                .depth_image = std::move(depth_image),
                                                .depth_image_memory = std::move(depth_memory),
                                                .depth_image_view = std::move(depth_view),
                                                .render_finished = device_.create_semaphore(),
                                                .framebuffer = device_.create_framebuffer(fb_info)};
                }) |
            std::ranges::to<std::vector>();
    }
} // namespace retro
