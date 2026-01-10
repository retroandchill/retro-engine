/**
 * @file actor_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    boost::optional<Viewport &> ActorHandleResolver<Viewport>::resolve(const ViewportID id)
    {
        return Engine::instance().scene().get_entity(id);
    }

    boost::optional<RenderObject &> ActorHandleResolver<RenderObject>::resolve(RenderObjectID id)
    {
        return Engine::instance().scene().get_render_object(id);
    }
} // namespace retro
