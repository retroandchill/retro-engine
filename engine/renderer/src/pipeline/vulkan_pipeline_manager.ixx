/**
 * @file vulkan_pipeline_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.vulkan_pipeline_manager;

import std;
import retro.core;
import retro.runtime;
import vulkan_hpp;
import :components;
import :pipeline.vulkan_render_pipeline;

namespace retro
{
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
