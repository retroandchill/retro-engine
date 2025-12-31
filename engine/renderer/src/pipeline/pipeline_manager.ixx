//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.pipeline_manager;

import std;
import retro.core;
import :pipeline.render_pipeline;
import vulkan_hpp;
import :components;

namespace retro
{
    export class RETRO_API PipelineManager
    {
      public:
        explicit inline PipelineManager(const vk::Device device,
                                        const VulkanSwapchain &swapchain,
                                        const vk::RenderPass render_pass)
            : device_{device}, cache_{device.createPipelineCacheUnique(vk::PipelineCacheCreateInfo{})},
              pipeline_(device, swapchain, render_pass)
        {
        }

        inline void recreate_pipelines(const VulkanSwapchain &swapchain, const vk::RenderPass render_pass)
        {
            pipeline_.recreate(device_, swapchain, render_pass);
        }

        inline void draw_quad(const Vector2f position, const Vector2f size, const Color color)
        {
            pipeline_.draw_quad(position, size, color);
        }

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size);

      private:
        vk::Device device_;
        vk::UniquePipelineCache cache_;

        RenderPipeline pipeline_;
    };
} // namespace retro
