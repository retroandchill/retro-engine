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

module retro.renderer.vulkan.scopes.presenter;

import retro.renderer.vulkan.backends.sdl;
import retro.core.containers.optional;
import retro.core.math.vector;

namespace retro
{
    namespace
    {

        Optional<std::uint32_t> get_present_queue_family_index(vk::PhysicalDevice physical_device,
                                                               vk::SurfaceKHR surface)
        {
            for (const auto [i, family] : physical_device.getQueueFamilyProperties() | std::views::enumerate)
            {
                vk::Bool32 present_support = vk::False;
                if (const auto res = physical_device.getSurfaceSupportKHR(i, surface, &present_support);
                    res == vk::Result::eSuccess && present_support == vk::True)
                {
                    return i;
                }
            }

            return std::nullopt;
        }

        vk::UniqueRenderPass create_render_pass(vk::Device device,
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

        std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
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
    } // namespace

    VulkanPresenter::VulkanPresenter(Window &window,
                                     vk::SurfaceKHR surface,
                                     VulkanDeviceHandles device_handles,
                                     VulkanSwapchain swapchain,
                                     vk::UniqueRenderPass render_pass,
                                     std::vector<vk::UniqueFramebuffer> framebuffers,
                                     std::vector<vk::UniqueCommandBuffer> command_buffers,
                                     VulkanSyncObjects sync,
                                     VulkanPipelineManager &pipeline_manager,
                                     VulkanBufferManager &buffer_manager)
        : window_(window), surface_(surface), device_handles_{device_handles}, swapchain_(std::move(swapchain)),
          render_pass_(std::move(render_pass)), framebuffers_(std::move(framebuffers)),
          command_buffers_(std::move(command_buffers)), sync_(std::move(sync)), pipeline_manager_{pipeline_manager_},
          buffer_manager_{buffer_manager_}
    {
    }

    VulkanPresenter VulkanPresenter::create(const VulkanPresenterCreateInfo &info)
    {
        auto swapchain = VulkanSwapchain{SwapchainConfig{
            .physical_device = info.device_handles.physical_device,
            .device = info.device_handles.device,
            .surface = info.surface,
            .graphics_family = info.device_handles.graphics_family_index,
            .present_family = info.device_handles.present_family_index,
            .width = info.window.width(),
            .height = info.window.height(),
        }};
        auto render_pass =
            create_render_pass(info.device_handles.device, swapchain.format(), vk::SampleCountFlagBits::e1);
        auto framebuffers = create_framebuffers(info.device_handles.device, render_pass.get(), swapchain);
        auto command_buffers = info.device_handles.device.allocateCommandBuffersUnique(
            vk::CommandBufferAllocateInfo{.commandPool = info.command_pool,
                                          .level = vk::CommandBufferLevel::ePrimary,
                                          .commandBufferCount = MAX_FRAMES_IN_FLIGHT});

        auto sync = VulkanSyncObjects{SyncConfig{
            .device = info.device_handles.device,
            .frames_in_flight = MAX_FRAMES_IN_FLIGHT,
            .swapchain_image_count = static_cast<std::uint32_t>(swapchain.image_views().size()),
        }};

        return VulkanPresenter{info.window,
                               info.surface,
                               info.device_handles,
                               std::move(swapchain),
                               std::move(render_pass),
                               std::move(framebuffers),
                               std::move(command_buffers),
                               std::move(sync),
                               info.pipeline_manager,
                               info.buffer_manager};
    }

    void VulkanPresenter::begin_frame()
    {
        const auto dev = device_handles_.device;

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

    void VulkanPresenter::end_frame()
    {
        const auto in_flight = sync_.in_flight(current_frame_);
        auto cmd = command_buffers_[current_frame_].get();

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

        if (device_handles_.graphics_queue.submit(1, &submit_info, in_flight) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
        }

        std::array swapchains = {swapchain_.handle()};

        vk::PresentInfoKHR present_info{.waitSemaphoreCount = signal_semaphores.size(),
                                        .pWaitSemaphores = signal_semaphores.data(),
                                        .swapchainCount = swapchains.size(),
                                        .pSwapchains = swapchains.data(),
                                        .pImageIndices = &image_index_};

        if (const auto result = device_handles_.present_queue.presentKHR(&present_info);
            result == vk::Result::eErrorOutOfDateKHR || result == vk::Result::eSuboptimalKHR)
        {
            recreate_swapchain();
        }
        else if (result != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to present swapchain image"};
        }

        current_frame_ = (current_frame_ + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    void VulkanPresenter::recreate_swapchain()
    {
        const auto [w, h] = window_.size();
        if (w == 0 || h == 0)
            return;

        device_handles_.device.waitIdle();

        swapchain_ = VulkanSwapchain{SwapchainConfig{.physical_device = device_handles_.physical_device,
                                                     .device = device_handles_.device,
                                                     .surface = surface_,
                                                     .graphics_family = device_handles_.graphics_family_index,
                                                     .present_family = device_handles_.present_family_index,
                                                     .width = w,
                                                     .height = h,
                                                     .old_swapchain = swapchain_.handle()}};
        render_pass_ = create_render_pass(device_handles_.device, swapchain_.format(), vk::SampleCountFlagBits::e1);
        framebuffers_ = create_framebuffers(device_handles_.device, render_pass_.get(), swapchain_);
    }

    void VulkanPresenter::record_command_buffer(vk::CommandBuffer cmd, std::uint32_t image_index)
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
        pipeline_manager_.bind_and_render(cmd, window_.size(), sync_.descriptor_pool(current_frame_), buffer_manager_);
        cmd.endRenderPass();
        cmd.end();
    }
} // namespace retro
