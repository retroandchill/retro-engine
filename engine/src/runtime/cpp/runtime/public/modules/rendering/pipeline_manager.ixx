/**
 * @file pipeline_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.pipeline_manager;

import std;
import retro.core.di;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.renderer2d;
import retro.core.math.vector;
import retro.runtime.world.scene_node;
import retro.runtime.world.viewport;
import retro.core.memory.small_unique_ptr;
import retro.runtime.rendering.draw_command;

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
        static constexpr std::size_t default_pool_size = 1024 * 1024 * 16;

        explicit PipelineManager(std::span<RenderPipeline *> pipelines);

        [[nodiscard]] inline auto pipelines() const noexcept
        {
            return pipelines_ | std::views::transform([](const std::pair<std::type_index, PipelineUsage> &entry)
                                                      { return std::make_pair(entry.first, entry.second.pipeline); });
        }

        DrawCommandSet collect_draw_command_sources(const SceneNodeList &nodes,
                                                    Vector2u viewport_size,
                                                    const Viewport &viewport,
                                                    std::pmr::memory_resource &memory_resource);

      private:
        std::map<std::type_index, PipelineUsage> pipelines_{};
    };
} // namespace retro
