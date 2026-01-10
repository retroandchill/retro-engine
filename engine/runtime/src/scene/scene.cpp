/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    boost::optional<Viewport &> Scene2D::get_entity(const ViewportID id)
    {
        return viewports_.get(id);
    }

    Viewport &Scene2D::create_viewport() noexcept
    {
        return viewports_.emplace();
    }

    void Scene2D::destroy_viewport(const ViewportID id)
    {
        viewports_.remove(id);
    }

    boost::optional<RenderObject &> Scene2D::get_render_object(const RenderObjectID id)
    {
        return render_objects_.get(id).map([](ComponentHandle &component) -> RenderObject & { return *component; });
    }

    void Scene2D::destroy_render_object(const RenderObjectID render_object_id)
    {
        render_objects_.remove(render_object_id);
    }
} // namespace retro
