export module retro.interop:generated.entity_exporter;

import retro.core;    
import retro.runtime;    
import retro.scripting;    

namespace retro::entity_exporter 
{
    int32 get_entity_transform_offset();
    Entity& create_new_entity(const retro::Transform& transform, uint64& id);
    void remove_entity_from_scene(Entity& entityPtr);
}