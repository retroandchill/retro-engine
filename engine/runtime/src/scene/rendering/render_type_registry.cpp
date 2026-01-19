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

    void RenderTypeRegistry::unregister_pipeline(const Name name)
    {
        registrations_.erase(name);
    }

    std::vector<std::unique_ptr<RenderPipeline>> RenderTypeRegistry::create_pipelines() const
    {
        return registrations_ | std::views::values |
               std::views::transform([&](auto registration) { return registration.create_pipeline(); }) |
               std::ranges::to<std::vector>();
    }
} // namespace retro
