/**
 * @file entity_exporter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.interop:generated.entity_exporter;

import retro.core;
import retro.runtime;
import retro.scripting;

namespace retro::entity_exporter
{
    const Transform &get_entity_transform(EntityID entity_id);
    void set_entity_transform(EntityID entity_id, const Transform &transform);
    EntityID create_new_entity(const Transform &transform);
    void remove_entity_from_scene(EntityID entity_id);
} // namespace retro::entity_exporter
