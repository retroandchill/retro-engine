/**
 * @file entity_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.interop;

namespace retro::entity_exporter
{
    const Transform &get_entity_transform(const EntityID entity_id)
    {
        return Engine::instance().scene().get_entity(entity_id).value().transform();
    }

    void set_entity_transform(const EntityID entity_id, const Transform &transform)
    {
        Engine::instance().scene().get_entity(entity_id).value().set_transform(transform);
    }

    EntityID create_new_entity(const Transform &transform)
    {
        return Engine::instance().scene().create_entity(transform).id();
    }

    void remove_entity_from_scene(const EntityID entity_id)
    {
        Engine::instance().scene().destroy_entity(entity_id);
    }
} // namespace retro::entity_exporter
