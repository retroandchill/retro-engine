//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.render_pipeline;

import std;
import vulkan_hpp;
import :render_data;
import :components;

namespace retro
{
    export class RETRO_API RenderPipeline
    {
      public:
        inline RenderPipeline(const vk::Device device,
                              const VulkanSwapchain &swapchain,
                              const vk::RenderPass render_pass)
            : pipeline_layout_{create_pipeline_layout(device)},
              graphics_pipeline_{create_graphics_pipeline(device, pipeline_layout_.get(), swapchain, render_pass)}
        {
        }

        void recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass);
        void draw_quad(Vector2f position, Vector2f size, Color color);

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size);

      private:
        static vk::UniquePipelineLayout create_pipeline_layout(vk::Device device);
        static vk::UniquePipeline create_graphics_pipeline(vk::Device device,
                                                           vk::PipelineLayout layout,
                                                           const VulkanSwapchain &swapchain,
                                                           vk::RenderPass render_pass);
        static vk::UniqueShaderModule create_shader_module(vk::Device device, const std::filesystem::path &path);

        vk::UniquePipelineLayout pipeline_layout_;
        vk::UniquePipeline graphics_pipeline_;
        std::vector<Quad> pending_quads_;
    };
} // namespace retro
