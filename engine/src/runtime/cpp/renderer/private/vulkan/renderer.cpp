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

module retro.renderer.vulkan.renderer;

import retro.logging;
import vulkan_hpp;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(Window &window,
                                       const vk::SurfaceKHR surface,
                                       VulkanDevice &device,
                                       VulkanSwapchain &swapchain,
                                       VulkanBufferManager &buffer_manager,
                                       const vk::CommandPool command_pool,
                                       VulkanPipelineManager &pipeline_manager,
                                       ViewportRendererFactory &viewport_factory)
        : window_{window}, surface_{surface}, device_{device}, buffer_manager_{buffer_manager}, swapchain_(swapchain),
          command_pool_(command_pool), pipeline_manager_{pipeline_manager}, viewport_factory_{viewport_factory}
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

    VulkanRenderer2D::~VulkanRenderer2D()
    {
        // We need to ensure that there are no in-flight frames before any members are destroyed
        device_.wait_idle();
    }

    void VulkanRenderer2D::wait_for_current_frame()
    {
        const auto fence = frame_resources_.at(current_frame_).in_flight.get();
        device_.wait_for_fences(fence);
    }

    std::pmr::memory_resource &VulkanRenderer2D::get_next_frame_memory_resource()
    {
        // TODO: For now just return the default one
        return *std::pmr::get_default_resource();
    }

    void VulkanRenderer2D::push_next_frame_draw_commands(std::pmr::vector<DrawCommandSet> draw_command_sets)
    {
    }

    void VulkanRenderer2D::begin_frame()
    {
        current_frame_ = (current_frame_ + 1) % max_frames_in_flight;
        auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
        device_.wait_for_fences(in_flight);

        device_.reset_descriptor_pool(frame_resources_.at(current_frame_).descriptor_pool.get());

        auto result = device_.acquire_next_image(swapchain_.handle(),
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
            throw std::runtime_error{"VulkanRenderer2D: failed to acquire swapchain image"};
        }

        device_.reset_fences(in_flight);
    }

    void VulkanRenderer2D::end_frame()
    {
        const auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
        auto cmd = frame_resources_.at(current_frame_).command_buffer.get();

        cmd.reset();
        record_command_buffer(cmd, image_index_);

        std::array wait_semaphores = {frame_resources_.at(current_frame_).image_available.get()};
        std::array wait_stages = {
            static_cast<vk::PipelineStageFlags>(vk::PipelineStageFlagBits::eColorAttachmentOutput)};

        // Signal semaphore is now per-image
        const vk::Semaphore render_finished_semaphore =
            swapchain_.image_resources()[image_index_].render_finished.get();
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
                    throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
                }
            });

        std::array swapchains = {swapchain_.handle()};

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
                    throw std::runtime_error{"VulkanRenderer2D: failed to present swapchain image"};
                }
            });

        pipeline_manager_.clear_draw_queue();
        buffer_manager_.reset();
    }

    Window &VulkanRenderer2D::window() const
    {
        return window_;
    }

    void VulkanRenderer2D::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {
        pipeline_manager_.create_pipeline(type, pipeline, swapchain_, swapchain_.render_pass());
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

    void VulkanRenderer2D::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = window_.size();
        if (w == 0 || h == 0)
            return;

        device_.wait_idle();

        swapchain_.recreate(w, h);
        pipeline_manager_.recreate_pipelines(swapchain_);
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
            .renderPass = swapchain_.render_pass(),
            .framebuffer = swapchain_.image_resources()[image_index].framebuffer.get(),
            .renderArea = vk::Rect2D{.offset = vk::Offset2D{.x = 0, .y = 0},
                                     .extent = vk::Extent2D{.width = screen_width, .height = screen_height}},
            .clearValueCount = clear_values.size(),
            .pClearValues = clear_values.data()};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);

        for (const auto &viewport : viewports_)
        {
            viewport->render_viewport(cmd,
                                      Vector2u{screen_width, screen_height},
                                      frame_resources_.at(current_frame_).descriptor_pool.get());
        }

        cmd.endRenderPass();

        cmd.end();
    }

} // namespace retro
