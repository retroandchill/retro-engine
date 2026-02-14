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

module retro.renderer.vulkan.renderer;

import retro.logging;
import vulkan_hpp;

namespace retro
{
    namespace
    {
        vk::UniqueRenderPass create_render_pass(const vk::Device device,
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

            return device.createRenderPassUnique(rp_info);
        }

        std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
                                                               vk::RenderPass render_pass,
                                                               const VulkanSwapchain &swapchain)
        {
            return std::views::zip(swapchain.color_image_views(), swapchain.depth_image_views()) |
                   std::views::transform(
                       [device, render_pass, &swapchain](
                           const std::tuple<const vk::UniqueImageView &, const vk::UniqueImageView &> &images)
                       {
                           std::array attachments = {std::get<0>(images).get(), std::get<1>(images).get()};

                           const vk::FramebufferCreateInfo fb_info{.renderPass = render_pass,
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

    VulkanRenderer2D::VulkanRenderer2D(Window &window,
                                       const vk::SurfaceKHR surface,
                                       VulkanDevice &device,
                                       VulkanSwapchain &swapchain,
                                       VulkanBufferManager &buffer_manager,
                                       const vk::CommandPool command_pool,
                                       VulkanPipelineManager &pipeline_manager,
                                       ViewportRendererFactory &viewport_factory)
        : window_{window}, surface_{surface}, device_{device}, buffer_manager_{buffer_manager}, swapchain_(swapchain),
          render_pass_(create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1)),
          framebuffers_(create_framebuffers(device_.device(), render_pass_.get(), swapchain_)),
          command_pool_(command_pool), command_buffers_{device_.device().allocateCommandBuffersUnique(
                                           vk::CommandBufferAllocateInfo{.commandPool = command_pool,
                                                                         .level = vk::CommandBufferLevel::ePrimary,
                                                                         .commandBufferCount = max_frames_in_flight})},
          sync_(SyncConfig{
              .device = device_.device(),
              .frames_in_flight = max_frames_in_flight,
              .swapchain_image_count = static_cast<std::uint32_t>(swapchain_.color_image_views().size()),
          }),
          pipeline_manager_{pipeline_manager}, viewport_factory_{viewport_factory}
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

        current_frame_ = (current_frame_ + 1) % max_frames_in_flight;
        pipeline_manager_.clear_draw_queue();
        buffer_manager_.reset();
    }

    Window &VulkanRenderer2D::window() const
    {
        return window_;
    }

    void VulkanRenderer2D::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {
        pipeline_manager_.create_pipeline(type, pipeline, swapchain_, render_pass_.get());
    }

    void VulkanRenderer2D::remove_render_pipeline(const std::type_index type)
    {
        pipeline_manager_.destroy_pipeline(type);
    }

    void VulkanRenderer2D::add_viewport(Viewport &viewport)
    {
        viewports_.emplace_back(viewport_factory_.create(viewport));
        viewport.on_z_order_changed().add([this](Viewport &, std::int32_t) { viewports_sorted_ = false; });
        viewports_sorted_ = false;
    }

    void VulkanRenderer2D::remove_viewport(Viewport &viewport)
    {
        std::erase_if(viewports_,
                      [&](const std::unique_ptr<ViewportRenderer> &v)
                      { return std::addressof(v->viewport()) == std::addressof(viewport); });
    }

    vk::UniqueSampler VulkanRenderer2D::create_linear_sampler() const
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

        return device_.device().createSamplerUnique(sampler_info);
    }

    void VulkanRenderer2D::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = window_.size();
        if (w == 0 || h == 0)
            return;

        device_.device().waitIdle();

        swapchain_ = VulkanSwapchain{SwapchainConfig{.physical_device = device_.physical_device(),
                                                     .device = device_.device(),
                                                     .surface = surface_,
                                                     .graphics_family = device_.graphics_family_index(),
                                                     .present_family = device_.present_family_index(),
                                                     .width = w,
                                                     .height = h,
                                                     .old_swapchain = swapchain_.handle()}};
        render_pass_ = create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1);
        framebuffers_ = create_framebuffers(device_.device(), render_pass_.get(), swapchain_);
        pipeline_manager_.recreate_pipelines(swapchain_, render_pass_.get());
    }

    void VulkanRenderer2D::record_command_buffer(const vk::CommandBuffer cmd, const std::uint32_t image_index)
    {
        constexpr vk::CommandBufferBeginInfo begin_info{};

        cmd.begin(begin_info);

        if (!viewports_sorted_)
        {
            using PtrType = std::unique_ptr<ViewportRenderer>;
            std::ranges::sort(viewports_,
                              [](const PtrType &a, const PtrType &b)
                              { return a->viewport().z_order() < b->viewport().z_order(); });
            viewports_sorted_ = true;
        }

        // ReSharper disable once CppDFAUnusedValue
        // ReSharper disable once CppDFAUnreadVariable
        vk::ClearValue color_clear_value{.color = vk::ClearColorValue{.float32 = std::array{0.0f, 0.0f, 0.0f, 1.0f}}};
        vk::ClearValue depth_clear_value{.depthStencil = vk::ClearDepthStencilValue{.depth = 1.0f}};

        std::array clear_values = {color_clear_value, depth_clear_value};

        auto [screen_width, screen_height] = swapchain_.extent();

        const vk::RenderPassBeginInfo rp_info{
            .renderPass = render_pass_.get(),
            .framebuffer = framebuffers_.at(image_index).get(),
            .renderArea = vk::Rect2D{.offset = vk::Offset2D{.x = 0, .y = 0},
                                     .extent = vk::Extent2D{.width = screen_width, .height = screen_height}},
            .clearValueCount = clear_values.size(),
            .pClearValues = clear_values.data()};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);

        for (const auto &viewport : viewports_)
        {
            viewport->render_viewport(cmd,
                                      Vector2u{screen_width, screen_height},
                                      sync_.descriptor_pool(current_frame_));
        }

        cmd.endRenderPass();

        cmd.end();
    }

} // namespace retro
