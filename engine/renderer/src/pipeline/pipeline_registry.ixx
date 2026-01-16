/**
 * @file pipeline_registry.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.pipeline_registry;

import std;
import retro.core;
import :pipeline.render_pipeline;

namespace retro
{
    export using PipelineFactory =
        std::function<std::unique_ptr<RenderPipeline>(vk::Device, const VulkanSwapchain &, vk::RenderPass)>;

    export class RETRO_API PipelineRegistry
    {
        PipelineRegistry() = default;
        ~PipelineRegistry() = default;

      public:
        static PipelineRegistry &instance();

        void register_pipeline(Name name, PipelineFactory factory);
        void unregister_pipeline(Name name);
        [[nodiscard]] std::vector<std::unique_ptr<RenderPipeline>> create_pipelines(vk::Device device,
                                                                                    const VulkanSwapchain &swapchain,
                                                                                    vk::RenderPass render_pass) const;

      private:
        std::map<Name, PipelineFactory> factories_;
    };

    export struct PipelineRegistration
    {
        RETRO_API PipelineRegistration(Name pipelineName, PipelineFactory factory);
    };
} // namespace retro
