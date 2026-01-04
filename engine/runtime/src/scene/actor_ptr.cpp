/**
 * @file actor_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    boost::optional<Entity &> ActorHandleResolver<Entity>::resolve(const EntityID id)
    {
        return Engine::instance().scene().get_entity(id);
    }

    boost::optional<Component &> ActorHandleResolver<Component>::resolve(ComponentID id)
    {
        return Engine::instance().scene().get_component(id);
    }
} // namespace retro
