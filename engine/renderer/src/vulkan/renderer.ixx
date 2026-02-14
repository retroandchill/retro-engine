/**
 * @file renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.renderer;

import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.texture_manager;
import retro.runtime.rendering.renderer2d;
import retro.runtime.world.viewport;
import retro.renderer.vulkan.components.sync;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.viewport_renderer;
import retro.core.di;
import retro.core.math.vector;
import retro.platform.window;
import vulkan_hpp;
import std;

namespace retro
{
    export class VulkanRenderer2D final : public Renderer2D
    {
      public:
        static constexpr std::uint32_t max_frames_in_flight = 2;

        using Dependencies = TypeList<Window &,
                                      vk::SurfaceKHR,
                                      VulkanDevice &,
                                      VulkanSwapchain &,
                                      VulkanBufferManager &,
                                      vk::CommandPool,
                                      VulkanPipelineManager &,
                                      ViewportRendererFactory &>;

        explicit VulkanRenderer2D(Window &window,
                                  vk::SurfaceKHR surface,
                                  VulkanDevice &device,
                                  VulkanSwapchain &swapchain,
                                  VulkanBufferManager &buffer_manager,
                                  vk::CommandPool command_pool,
                                  VulkanPipelineManager &pipeline_manager,
                                  ViewportRendererFactory &viewport_factory);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        void wait_idle() override;

        void begin_frame() override;

        void end_frame() override;

        [[nodiscard]] Window &window() const override;

        void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

        void add_viewport(Viewport &viewport) override;

        void remove_viewport(Viewport &viewport) override;

      private:
        [[nodiscard]] vk::UniqueSampler create_linear_sampler() const;

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, std::uint32_t image_index);

        Window &window_;

        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        VulkanBufferManager &buffer_manager_;
        VulkanSwapchain &swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        vk::CommandPool command_pool_;
        std::vector<vk::UniqueCommandBuffer> command_buffers_;
        VulkanSyncObjects sync_;
        VulkanPipelineManager &pipeline_manager_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;

        ViewportRendererFactory &viewport_factory_;
        std::vector<std::unique_ptr<ViewportRenderer>> viewports_;
        bool viewports_sorted_ = false;
    };
} // namespace retro
