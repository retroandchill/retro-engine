/**
 * @file renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.renderer;

import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.renderer2d;
import retro.runtime.world.viewport;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.presenter;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.pipeline;
import retro.core.di;
import retro.core.math.vector;
import retro.platform.window;
import vulkan;
import std;
import retro.runtime.rendering.draw_command;
import retro.core.memory.arena_allocator;
import retro.core.memory.ref_counted_ptr;
import retro.renderer.vulkan.vulkan_render_backend;

namespace retro
{
    struct VulkanFrameResources
    {
        static constexpr std::size_t arena_size = 20 * 1024 * 1024;

        vk::UniqueCommandBuffer command_buffer;
        SingleArenaMemoryResource memory_resource{std::in_place, arena_size};

        vk::UniqueSemaphore image_available;
        vk::UniqueFence in_flight;
        vk::UniqueDescriptorPool descriptor_pool;
    };

    export class VulkanRenderer2D final : public Renderer2D
    {
      public:
        static constexpr std::uint32_t max_frames_in_flight = 2;

        explicit VulkanRenderer2D(VulkanRenderBackend &backend,
                                  std::shared_ptr<Window> window,
                                  vk::UniqueSurfaceKHR surface,
                                  VulkanDevice &device,
                                  VulkanBufferManager &buffer_manager,
                                  vk::CommandPool command_pool);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        ~VulkanRenderer2D() override;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        void request_stop() override;

        void wait_for_current_frame() override;

        void queue_frame_for_render(RenderQueueFn factory) override;

        void render_next_available_frame() override;

        [[nodiscard]] Window &window() const override;

        void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

      private:
        RefCountPtr<VulkanRenderBackend> backend_;
        std::shared_ptr<Window> window_;

        vk::UniqueSurfaceKHR surface_;
        VulkanDevice &device_;
        VulkanBufferManager &buffer_manager_;
        vk::CommandPool command_pool_;
        VulkanPipelineManager pipeline_manager_;
        VulkanPresenter presenter_;
        std::stop_source renderer_teardown_source_;
    };
} // namespace retro
