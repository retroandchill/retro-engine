/**
 * @file pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline;

import std;
import retro.runtime;
import vulkan_hpp;
import :components;

namespace retro
{
    export class RETRO_API VulkanRenderPipeline
    {
      public:
        inline VulkanRenderPipeline(std::shared_ptr<RenderPipeline> pipeline,
                                    vk::Device device,
                                    const VulkanSwapchain &swapchain,
                                    vk::RenderPass render_pass)
            : pipeline_{std::move(pipeline)}
        {
            recreate(device, swapchain, render_pass);
        }

        void clear_draw_queue();

        void recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass);

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size);

      private:
        [[nodiscard]] vk::UniquePipelineLayout create_pipeline_layout(vk::Device device) const;

        vk::UniquePipeline create_graphics_pipeline(vk::Device device,
                                                    vk::PipelineLayout layout,
                                                    const VulkanSwapchain &swapchain,
                                                    vk::RenderPass render_pass);

        static vk::UniqueShaderModule create_shader_module(vk::Device device, const std::filesystem::path &path);

        std::shared_ptr<RenderPipeline> pipeline_;
        vk::UniquePipelineLayout pipeline_layout_;
        vk::UniquePipeline graphics_pipeline_;
    };

    export class RETRO_API VulkanPipelineManager
    {
      public:
        explicit inline VulkanPipelineManager(const vk::Device device)
            : device_{device}, cache_{device.createPipelineCacheUnique(vk::PipelineCacheCreateInfo{})}
        {
        }

        void recreate_pipelines(const VulkanSwapchain &swapchain, vk::RenderPass render_pass);

        void create_pipeline(std::type_index type,
                             std::shared_ptr<RenderPipeline> pipeline,
                             const VulkanSwapchain &swapchain,
                             vk::RenderPass render_pass);
        void destroy_pipeline(std::type_index type);

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size);
        void clear_draw_queue();

      private:
        vk::Device device_;
        vk::UniquePipelineCache cache_;

        std::vector<VulkanRenderPipeline> pipelines_;
        std::map<std::type_index, usize> pipeline_indices_;
    };
} // namespace retro
