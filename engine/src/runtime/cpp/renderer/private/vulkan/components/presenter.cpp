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

    VulkanPresenter::VulkanPresenter(VulkanRenderTarget &render_target,
                                     VulkanDevice &device,
                                     const vk::CommandPool command_pool,
                                     VulkanPipelineManager &pipeline_manager)
        : vulkan_render_target_{render_target}, device_{device}, command_pool_{command_pool},
          pipeline_manager_{pipeline_manager}
    {

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

    void VulkanPresenter::queue_frame_for_render(RenderQueueFn factory, const std::stop_token &stop_token)
    {
        pending_frame_slots_.produce(
            [factory](PendingFrameSlot &slot)
            {
                slot.pending_commands = factory(slot.memory_resource);
                std::ranges::sort(slot.pending_commands,
                                  [](const DrawCommandSet &lhs, const DrawCommandSet &rhs)
                                  { return lhs.z_order < rhs.z_order; });
            },
            stop_token);
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

        vulkan_render_target_.acquire_next_image(frame_resources_.at(current_frame_).image_available.get(),
                                                 image_index_);
    }

    void VulkanPresenter::submit_and_present(const std::stop_token &stop_token)
    {
        const auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
        auto cmd = frame_resources_.at(current_frame_).command_buffer.get();
        cmd.reset();
        record_command_buffer(cmd, stop_token);

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

        vulkan_render_target_.present(image_index_, signal_semaphores);
    }

    void VulkanPresenter::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {

        pipeline_manager_.create_pipeline(type, pipeline, extent_, render_pass_.get());
    }

    void VulkanPresenter::remove_render_pipeline(const std::type_index type) const
    {
        pipeline_manager_.destroy_pipeline(type);
    }

    void VulkanPresenter::record_command_buffer(vk::CommandBuffer cmd, const std::stop_token &stop_token)
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
            },
            stop_token);
    }
} // namespace retro
