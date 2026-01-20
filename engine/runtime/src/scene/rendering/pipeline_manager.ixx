/**
 * @file pipeline_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.pipeline_manager;

import std;
import entt;
import retro.core;
import :scene.rendering.render_pipeline;
import :interfaces;

namespace retro
{
    export class RETRO_API PipelineManager
    {
      public:
        static constexpr usize DEFAULT_POOL_SIZE = 1024 * 1024 * 16;

        explicit PipelineManager(Renderer2D *renderer, const usize pool_size = DEFAULT_POOL_SIZE)
            : renderer_{renderer}, arena_{pool_size}
        {
        }

        template <RenderComponent Component>
        void set_up_pipeline_listener(entt::registry &registry)
        {
            registry.on_construct<Component>().template connect<&PipelineManager::on_component_added<Component>>(this);
            registry.on_destroy<Component>().template connect<&PipelineManager::on_component_removed<Component>>(this);
        }

        void reset_arena();

        void collect_all_draw_calls(const entt::registry &registry, Vector2u viewport_size);

      private:
        template <RenderComponent Component>
        void on_component_added(entt::registry &, entt::entity)
        {
            using Pipeline = Component::PipelineType;
            auto type_index = std::type_index(typeid(Pipeline));

            if (usage_counts_[type_index]++ == 0)
            {
                auto pipeline = std::make_shared<Pipeline>();
                active_pipelines_[type_index] = pipeline;
                renderer_->add_new_render_pipeline(type_index, std::move(pipeline));
            }
        }

        template <RenderComponent Component>
        void on_component_removed(entt::registry &, entt::entity)
        {
            using Pipeline = Component::PipelineType;
            auto type_index = std::type_index(typeid(Pipeline));

            if (--usage_counts_[type_index] == 0)
            {
                active_pipelines_.erase(type_index);
                renderer_->remove_render_pipeline(type_index);
            }
        }

        Renderer2D *renderer_{};
        std::map<std::type_index, std::shared_ptr<RenderPipeline>> active_pipelines_{};
        std::map<std::type_index, usize> usage_counts_{};
        SingleArena arena_{DEFAULT_POOL_SIZE};
    };
} // namespace retro
