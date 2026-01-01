//
// Created by fcors on 1/1/2026.
//
module;

#include <stddef.h>

module retro.interop;

namespace retro::entity_exporter
{
    int32 get_entity_transform_offset()
    {
        return Entity::transform_offset();
    }

    Entity& create_new_entity(const Transform &transform, uint64 &id)
    {
        auto &entity = Engine::instance().scene().create_entity(transform);
        id = entity.id();
        return entity;
    }

    void remove_entity_from_scene(Entity &entityPtr)
    {
    }
}
