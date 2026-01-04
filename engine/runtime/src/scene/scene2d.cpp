/**
 * @file scene2d.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    boost::optional<Entity &> Scene2D::get_entity(const EntityID id)
    {
        return entities_.get(id);
    }

    Entity &Scene2D::create_entity(const Transform &transform) noexcept
    {
        return entities_.emplace(transform);
    }

    void Scene2D::destroy_entity(const EntityID id)
    {
        entities_.remove(id);
    }

    boost::optional<Component &> Scene2D::get_component(const ComponentID id)
    {
        return components_.get(id).map([](const std::unique_ptr<Component> &component) -> Component &
                                       { return *component; });
    }

    void Scene2D::destroy_component(const ComponentID component_id)
    {
        components_.remove(component_id);
    }
} // namespace retro
