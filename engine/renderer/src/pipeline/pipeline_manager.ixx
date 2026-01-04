/**
 * @file pipeline_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.pipeline_manager;

import std;
import retro.core;
import :pipeline.render_pipeline;
import vulkan_hpp;
import :components;
import :pipeline.pipeline_registry;

namespace retro
{
    export class RETRO_API PipelineManager
    {
      public:
        explicit inline PipelineManager(const vk::Device device,
                                        const VulkanSwapchain &swapchain,
                                        const vk::RenderPass render_pass)
            : device_{device}, cache_{device.createPipelineCacheUnique(vk::PipelineCacheCreateInfo{})},
              pipelines_(PipelineRegistry::instance().create_pipelines(device, swapchain, render_pass))
        {
            for (usize i = 0; i < pipelines_.size(); ++i)
            {
                pipeline_indices_[pipelines_[i]->type()] = i;
            }
        }

        inline void recreate_pipelines(const VulkanSwapchain &swapchain, const vk::RenderPass render_pass)
        {
            for (const auto &pipeline : pipelines_)
            {
                pipeline->recreate(device_, swapchain, render_pass);
            }
        }

        inline void queue_draw_calls(const Name type, const std::any &render_data)
        {
            if (const auto it = pipeline_indices_.find(type); it != pipeline_indices_.end())
            {
                pipelines_[it->second]->queue_draw_calls(render_data);
            }
        }

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size) const;

        void clear_draw_queue();

      private:
        vk::Device device_;
        vk::UniquePipelineCache cache_;

        std::vector<std::unique_ptr<RenderPipeline>> pipelines_;
        std::map<Name, usize> pipeline_indices_;
    };
} // namespace retro
