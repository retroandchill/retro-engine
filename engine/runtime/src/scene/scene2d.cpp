//
// Created by fcors on 1/1/2026.
//

module retro.runtime;

namespace retro
{
    Entity &Scene2D::create_entity(const Transform &transform) noexcept
    {
        return *entities_.emplace(transform);
    }

    void Scene2D::destroy_entity(EntityID id)
    {
        entities_.remove(id);
    }
} // namespace retro
