/**
 * @file entity_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.interop;

namespace retro::entity_exporter
{
    int32 get_entity_transform_offset()
    {
        return Entity::transform_offset();
    }

    Entity &create_new_entity(const Transform &transform, EntityID &id)
    {
        auto &entity = Engine::instance().scene().create_entity(transform);
        id = entity.id();
        return entity;
    }

    void remove_entity_from_scene(const Entity &entityPtr)
    {
        Engine::instance().scene().destroy_entity(entityPtr.id());
    }
} // namespace retro::entity_exporter
