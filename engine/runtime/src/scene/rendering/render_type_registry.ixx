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

namespace retro
{
    export struct RenderTypeRegistration
    {
        Name name;
        std::function<entt::entity(entt::registry &)> create_entity;
        std::function<std::unique_ptr<RenderPipeline>()> create_pipeline;
    };

    export class RETRO_API RenderTypeRegistry
    {
        RenderTypeRegistry() = default;
        ~RenderTypeRegistry() = default;

      public:
        static RenderTypeRegistry &instance();

        template <RenderType T>
        void register_type()
        {
            registrations_.emplace(T::name(),
                                   RenderTypeRegistration{.name = T::name(),
                                                          .create_entity =
                                                              [](entt::registry &registry)
                                                          {
                                                              auto entity = registry.create();
                                                              registry.emplace<typename T::Component>(entity);
                                                              return entity;
                                                          },
                                                          .create_pipeline =
                                                              []
                                                          {
                                                              return std::make_unique<T::Pipeline>();
                                                          }});
        }

        void unregister_pipeline(Name name);
        [[nodiscard]] std::vector<std::unique_ptr<RenderPipeline>> create_pipelines() const;

      private:
        std::map<Name, RenderTypeRegistration> registrations_{};
    };

    export template <RenderType T>
    struct PipelineRegistration
    {
        explicit PipelineRegistration()
        {
            RenderTypeRegistry::instance().register_type<T>();
        }
    };
} // namespace retro
