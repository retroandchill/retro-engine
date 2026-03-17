/**
 * @file presenter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.presenter;

import vulkan;
import retro.core.di;
import retro.core.functional.delegate;
import retro.core.memory.arena_allocator;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.pipeline;
import retro.platform.window;
import retro.runtime.rendering.render_pipeline;
import retro.core.math.vector;
import retro.core.util.deferred;
import retro.runtime.rendering.renderer2d;
import retro.core.containers.optional;
import retro.runtime.rendering.draw_command;
import retro.core.containers.spsc_circular_queue;

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
        vk::UniqueCommandBuffer command_buffer;
        vk::UniqueSemaphore image_available;
        vk::UniqueFence in_flight;
        vk::UniqueDescriptorPool descriptor_pool;
    };

    struct PendingFrameSlot
    {
        static constexpr std::size_t arena_size = 20 * 1024 * 1024;

        SingleArenaMemoryResource memory_resource{std::in_place, arena_size};
        std::pmr::vector<DrawCommandSet> pending_commands{&memory_resource};

        void reset() noexcept;
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

        void queue_frame_for_render(RenderQueueFn factory, const std::stop_token &stop_token);

        void begin_frame();

        void submit_and_present(const std::stop_token &stop_token);

        void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline);

        void remove_render_pipeline(std::type_index type) const;

        void recreate_swapchain();

      private:
        void record_command_buffer(vk::CommandBuffer cmd, const std::stop_token &stop_token);

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
        SpscCircularQueue<PendingFrameSlot, 3> pending_frame_slots_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;
    };
} // namespace retro
