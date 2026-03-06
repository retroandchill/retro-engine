/**
 * @file presenter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.presenter;

import vulkan_hpp;
import retro.core.di;
import retro.core.functional.delegate;
import retro.core.memory.arena_allocator;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.pipeline;
import retro.platform.window;
import retro.runtime.rendering.render_pipeline;
import retro.core.math.vector;
import retro.core.util.deferred;

namespace retro
{
    struct VulkanImageResources
    {
        vk::Image color_image;
        vk::UniqueImageView color_image_view;
        vk::UniqueImage depth_image;
        vk::UniqueDeviceMemory depth_image_memory;
        vk::UniqueImageView depth_image_view;
        vk::UniqueSemaphore render_finished;
        vk::UniqueFramebuffer framebuffer;
    };

    struct VulkanFrameResources
    {
        static constexpr std::size_t arena_size = 20 * 1024 * 1024;

        vk::UniqueCommandBuffer command_buffer;
        SingleArenaMemoryResource memory_resource{std::in_place, arena_size};

        vk::UniqueSemaphore image_available;
        vk::UniqueFence in_flight;
        vk::UniqueDescriptorPool descriptor_pool;
    };

    export struct VulkanSubmitContext
    {
        vk::CommandBuffer command_buffer;
        Vector2u screen_size;
        vk::DescriptorPool descriptor_pool;
    };

    export class VulkanPresenter
    {

        static constexpr std::uint32_t max_frames_in_flight = 2;

      public:
        using Dependencies =
            TypeList<Window &, vk::SurfaceKHR, VulkanDevice &, vk::CommandPool, VulkanPipelineManager &>;

        explicit VulkanPresenter(Window &window,
                                 vk::SurfaceKHR surface,
                                 VulkanDevice &device,
                                 vk::CommandPool command_pool,
                                 VulkanPipelineManager &manager);

        void wait_for_current_frame();

        void begin_frame();

        template <std::invocable<VulkanSubmitContext> Functor>
        void submit_and_present(Functor &&functor)
        {
            const auto in_flight = frame_resources_.at(current_frame_).in_flight.get();
            auto cmd = frame_resources_.at(current_frame_).command_buffer.get();
            cmd.reset();
            record_command_buffer(cmd, std::forward<Functor>(functor));

            submit_render_and_present(in_flight, cmd);
        }

        void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline);

        void remove_render_pipeline(std::type_index type) const;

        void recreate_swapchain();

      private:
        template <std::invocable<VulkanSubmitContext> Functor>
        void record_command_buffer(vk::CommandBuffer cmd, Functor &&functor)
        {
            constexpr vk::CommandBufferBeginInfo begin_info{};

            cmd.begin(begin_info);
            Deferred end_cmd{[cmd]
                             {
                                 cmd.end();
                             }};

            // ReSharper disable once CppDFAUnusedValue
            // ReSharper disable once CppDFAUnreadVariable
            vk::ClearValue color_clear_value{.color =
                                                 vk::ClearColorValue{.float32 = std::array{0.0f, 0.0f, 0.0f, 1.0f}}};
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

            std::invoke(std::forward<Functor>(functor),
                        VulkanSubmitContext{
                            .command_buffer = cmd,
                            .screen_size = {screen_width, screen_height},
                            .descriptor_pool = frame_resources_.at(current_frame_).descriptor_pool.get(),
                        });
        }

        void submit_render_and_present(vk::Fence in_flight, vk::CommandBuffer cmd);

        void create_swapchain(std::uint32_t width, std::uint32_t height);

        Window &window_;
        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        vk::CommandPool command_pool_;
        VulkanPipelineManager &pipeline_manager_;

        vk::UniqueSwapchainKHR swapchain_;
        vk::UniqueRenderPass render_pass_;
        vk::Format format_{vk::Format::eUndefined};
        vk::Extent2D extent_{};

        std::vector<VulkanImageResources> image_resources_;
        std::array<VulkanFrameResources, max_frames_in_flight> frame_resources_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;
    };
} // namespace retro
