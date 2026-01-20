/**
 * @file pipeline_registry.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.pipeline_registry;

import std;
import retro.core;
import :scene.rendering.render_pipeline;
import :scene.rendering.pipeline_manager;

namespace retro
{
    export class RETRO_API RenderTypeRegistry
    {
        RenderTypeRegistry() = default;
        ~RenderTypeRegistry() = default;

      public:
        static RenderTypeRegistry &instance();

        template <RenderComponent T>
        void register_type()
        {
            registrations_.emplace_back([](entt::registry &registry, PipelineManager &pipeline_manager)
                                        { pipeline_manager.set_up_pipeline_listener<T>(registry); });
        }

        void register_listeners(entt::registry &registry, PipelineManager &pipeline_manager) const;

      private:
        std::vector<std::function<void(entt::registry &, PipelineManager &)>> registrations_{};
    };

    export template <RenderComponent T>
    struct PipelineRegistration
    {
        explicit PipelineRegistration()
        {
            RenderTypeRegistry::instance().register_type<T>();
        }
    };
} // namespace retro
