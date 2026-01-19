/**
 * @file render_object_registry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    RenderObjectRegistry &RenderObjectRegistry::instance()
    {
        static RenderObjectRegistry registry{};
        return registry;
    }

    void RenderObjectRegistry::register_type(const Name type_name, RenderObjectFactory creator)
    {
        factories_[type_name] = std::move(creator);
    }

    entt::entity RenderObjectRegistry::create(const Name type_name, const entt::entity viewport_id) const
    {
        const auto factory = factories_.find(type_name);
        if (factory == factories_.end())
            throw std::runtime_error("No factory registered for type: " + type_name.to_string());

        return factory->second(viewport_id);
    }

    RenderObjectTypeRegistration::RenderObjectTypeRegistration(const Name type_name, RenderObjectFactory creator)
    {
        RenderObjectRegistry::instance().register_type(type_name, std::move(creator));
    }
} // namespace retro
