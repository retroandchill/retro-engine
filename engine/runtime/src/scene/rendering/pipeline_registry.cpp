/**
 * @file pipeline_registry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    PipelineRegistry &PipelineRegistry::instance()
    {
        static PipelineRegistry instance;
        return instance;
    }

    void PipelineRegistry::register_pipeline(const Name name, PipelineFactory factory)
    {
        factories_[name] = std::move(factory);
    }

    void PipelineRegistry::unregister_pipeline(const Name name)
    {
        factories_.erase(name);
    }

    std::vector<std::unique_ptr<RenderPipeline>> PipelineRegistry::create_pipelines() const
    {
        return factories_ | std::views::values | std::views::transform([&](auto factory) { return factory(); }) |
               std::ranges::to<std::vector>();
    }

    PipelineRegistration::PipelineRegistration(const Name pipelineName, PipelineFactory factory)
    {
        PipelineRegistry::instance().register_pipeline(pipelineName, std::move(factory));
    }
} // namespace retro
