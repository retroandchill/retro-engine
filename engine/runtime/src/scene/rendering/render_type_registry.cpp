/**
 * @file pipeline_registry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    RenderTypeRegistry &RenderTypeRegistry::instance()
    {
        static RenderTypeRegistry instance;
        return instance;
    }

    void RenderTypeRegistry::register_listeners(entt::registry &registry, PipelineManager &pipeline_manager) const
    {
        for (auto &registration : registrations_)
        {
            registration(registry, pipeline_manager);
        }
    }
} // namespace retro
