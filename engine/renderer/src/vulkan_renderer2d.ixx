/**
 * @file vulkan_renderer2d.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <complex.h>
#include <entt/entity/registry.hpp>

export module retro.renderer:vulkan_renderer2d;

import vulkan_hpp;
import retro.runtime;
import :vulkan_viewport;
import :components;
import :pipeline;
import :vulkan_buffer_manager;

namespace retro
{
    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        explicit VulkanRenderer2D(std::shared_ptr<VulkanViewport> viewport);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        ~VulkanRenderer2D() override;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;
        void begin_frame() override;

        void end_frame() override;

        [[nodiscard]] Vector2u viewport_size() const override;

        void add_new_render_pipeline(std::type_index type, std::shared_ptr<RenderPipeline> pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

      private:
        static vk::UniqueInstance create_instance();
        static std::span<const char *const> get_required_instance_extensions();
        static vk::UniqueRenderPass create_render_pass(vk::Device device,
                                                       vk::Format color_format,
                                                       vk::SampleCountFlagBits samples);
        static std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
                                                                      vk::RenderPass render_pass,
                                                                      const VulkanSwapchain &swapchain);

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, uint32 image_index);

      private:
        std::shared_ptr<VulkanViewport> viewport_;

        vk::UniqueInstance instance_;
        vk::UniqueSurfaceKHR surface_;
        VulkanDevice device_;
        VulkanBufferManagerScope buffer_manager_;
        VulkanSwapchain swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        VulkanCommandPool command_pool_;
        VulkanSyncObjects sync_;
        VulkanPipelineManager pipeline_manager_;

        uint32 current_frame_ = 0;
        uint32 image_index_ = 0;

        static constexpr uint32 MAX_FRAMES_IN_FLIGHT = 2;
    };
} // namespace retro
