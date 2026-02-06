/**
 * @file pipeline_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.pipeline_manager;

import retro.core.di;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.renderer2d;
import retro.core.math.vector;
import retro.runtime.world.scene_node;
import std;

namespace retro
{
    struct PipelineUsage
    {
        RenderPipeline *pipeline;
        std::size_t usage_count;
    };

    export class RETRO_API PipelineManager
    {
      public:
        using Dependencies = TypeList<Renderer2D, RenderPipeline>;
        static constexpr std::size_t DEFAULT_POOL_SIZE = 1024 * 1024 * 16;

        explicit PipelineManager(Renderer2D &renderer, const std::vector<RenderPipeline *> &pipelines);

        void collect_all_draw_calls(const SceneNodeList &nodes, Vector2u viewport_size);

      private:
        Renderer2D &renderer_;
        std::map<std::type_index, PipelineUsage> pipelines_{};
    };
} // namespace retro
