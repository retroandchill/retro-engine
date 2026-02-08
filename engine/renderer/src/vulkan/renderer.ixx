/**
 * @file renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.renderer;

import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.texture_render_data;
import retro.runtime.rendering.renderer2d;
import retro.renderer.vulkan.components.sync;
import retro.renderer.vulkan.components.command_pool;
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
        static constexpr std::uint32_t MAX_FRAMES_IN_FLIGHT = 2;

        using Dependencies = TypeList<Window,
                                      vk::SurfaceKHR,
                                      VulkanDevice,
                                      VulkanSwapchain,
                                      VulkanBufferManager,
                                      VulkanCommandPool,
                                      VulkanPipelineManager,
                                      ViewportRendererFactory>;

        explicit VulkanRenderer2D(Window &window,
                                  vk::SurfaceKHR surface,
                                  VulkanDevice &device,
                                  VulkanSwapchain &swapchain,
                                  VulkanBufferManager &buffer_manager,
                                  VulkanCommandPool &command_pool,
                                  VulkanPipelineManager &pipeline_manager,
                                  ViewportRendererFactory &viewport_factory);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        void wait_idle() override;

        void begin_frame() override;

        void end_frame() override;

        [[nodiscard]] Vector2u viewport_size() const override;

        void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

        std::unique_ptr<TextureRenderData> upload_texture(const ImageData &image_data) override;

      private:
        [[nodiscard]] vk::UniqueSampler create_linear_sampler() const;

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, std::uint32_t image_index);

        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands() const;
        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

        static void transition_image_layout(vk::CommandBuffer cmd,
                                            vk::Image image,
                                            vk::ImageLayout old_layout,
                                            vk::ImageLayout new_layout);

        Window &window_;

        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        VulkanBufferManager &buffer_manager_;
        VulkanSwapchain &swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        VulkanCommandPool &command_pool_;
        VulkanSyncObjects sync_;
        vk::UniqueSampler linear_sampler_;
        VulkanPipelineManager &pipeline_manager_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;

        ViewportRendererFactory &viewport_factory_;
        std::vector<std::unique_ptr<ViewportRenderer>> viewports_;
    };
} // namespace retro
