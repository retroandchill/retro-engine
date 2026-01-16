/**
 * @file vulkan_renderer2d.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

import retro.core;
import retro.logging;
import vulkan_hpp;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(std::shared_ptr<VulkanViewport> viewport)
        : viewport_{std::move(viewport)}, instance_{create_instance()},
          surface_{viewport_->create_surface(instance_.get())}, device_{instance_.get(), surface_.get()},
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
              .swapchain_image_count = static_cast<uint32>(swapchain_.image_views().size()),
          }),
          pipeline_manager_{device_.device(), swapchain_, render_pass_.get()}
    {
    }

    VulkanRenderer2D::~VulkanRenderer2D()
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
        if (dev.waitForFences(1, &in_flight, vk::True, std::numeric_limits<uint64>::max()) == vk::Result::eTimeout)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to wait for fence"};
        }

        auto result = dev.acquireNextImageKHR(swapchain_.handle(),
                                              std::numeric_limits<uint64>::max(),
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

        const vk::SubmitInfo submit_info{wait_semaphores.size(),
                                         wait_semaphores.data(),
                                         wait_stages.data(),
                                         1,
                                         &cmd,
                                         signal_semaphores.size(),
                                         signal_semaphores.data()};

        if (device_.graphics_queue().submit(1, &submit_info, in_flight) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
        }

        std::array swapchains = {swapchain_.handle()};

        vk::PresentInfoKHR present_info{signal_semaphores.size(),
                                        signal_semaphores.data(),
                                        swapchains.size(),
                                        swapchains.data(),
                                        &image_index_};

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
    }

    void VulkanRenderer2D::queue_draw_calls(const Name type, const std::any &data)
    {
        pipeline_manager_.queue_draw_calls(type, data);
    }

    Vector2u VulkanRenderer2D::viewport_size() const
    {
        return viewport_->size();
    }

    vk::UniqueInstance VulkanRenderer2D::create_instance()
    {
        vk::ApplicationInfo app_info{"Retro Engine",
                                     vk::makeVersion(1, 0, 0),
                                     "Retro Engine",
                                     vk::makeVersion(1, 0, 0),
                                     vk::makeApiVersion(0, 1, 2, 0)};

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

        const auto extensions = get_required_instance_extensions();

        std::vector validation_feature_enables = {vk::ValidationFeatureEnableEXT::eDebugPrintf};

        vk::ValidationFeaturesEXT validation_features{static_cast<uint32>(validation_feature_enables.size()),
                                                      validation_feature_enables.data()};

        vk::InstanceCreateInfo create_info{{},
                                           &app_info,
                                           static_cast<uint32>(enabled_layers.size()),
                                           enabled_layers.data(),
                                           static_cast<uint32>(extensions.size()),
                                           extensions.data(),
                                           &validation_features};

        return vk::createInstanceUnique(create_info);
    }

    std::span<const char *const> VulkanRenderer2D::get_required_instance_extensions()
    {
        // Ask SDL what Vulkan instance extensions are required for this window
        uint32 count = 0;
        auto *names = sdl::vulkan::GetInstanceExtensions(&count);
        if (names == nullptr)
        {
            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
        }

        return std::span{names, count};
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

        vk::RenderPassCreateInfo rp_info{{}, 1, &color_attachment, 1, &subpass, 1, &dependency};

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

                       vk::FramebufferCreateInfo fb_info{{},
                                                         render_pass,
                                                         attachments.size(),
                                                         attachments.data(),
                                                         swapchain.extent().width,
                                                         swapchain.extent().height,
                                                         1};

                       return device.createFramebufferUnique(fb_info);
                   }) |
               std::ranges::to<std::vector>();
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

    void VulkanRenderer2D::record_command_buffer(const vk::CommandBuffer cmd, const uint32 image_index)
    {
        constexpr vk::CommandBufferBeginInfo begin_info{};

        cmd.begin(begin_info);
        vk::ClearValue clear{{0.0f, 0.0f, 0.0f, 1.0f}};

        const vk::RenderPassBeginInfo rp_info{render_pass_.get(),
                                              framebuffers_.at(image_index).get(),
                                              vk::Rect2D{vk::Offset2D{0, 0}, swapchain_.extent()},
                                              1,
                                              &clear};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);
        pipeline_manager_.bind_and_render(cmd, viewport_->size());
        cmd.endRenderPass();
        cmd.end();
    }
} // namespace retro
