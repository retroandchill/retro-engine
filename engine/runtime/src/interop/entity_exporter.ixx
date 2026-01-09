export module retro.runtime.interop:entity_exporter;

import retro.core;
import retro.runtime;

namespace retro::entity_exporter
{
    export const retro::Transform &get_entity_transform(EntityID entityId);
    export void set_entity_transform(EntityID entityId, const retro::Transform &transform);
    export EntityID create_new_entity(const retro::Transform &transform);
    export void remove_entity_from_scene(EntityID entityId);
} // namespace retro::entity_exporter
