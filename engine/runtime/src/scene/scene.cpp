/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import retro.core;

namespace retro
{
    boost::optional<Viewport &> Scene::get_viewport(const ViewportID id)
    {
        return viewports_.get(id);
    }

    Viewport &Scene::create_viewport() noexcept
    {
        return viewports_.emplace();
    }

    void Scene::destroy_viewport(const ViewportID id)
    {
        viewports_.remove(id);
    }

    boost::optional<RenderObject &> Scene::get_render_object(const RenderObjectID id)
    {
        return render_objects_.get(id).map([](RenderObjectHandle &component) -> RenderObject & { return *component; });
    }

    void Scene::destroy_render_object(const RenderObjectID render_object_id)
    {
        render_objects_.remove(render_object_id);
    }

    void Scene::process_scene_events()
    {
        const auto callback =
            Overload{[&](const DestroyRenderObject &event) { destroy_render_object(event.render_object_id); },
                     [&](const DestroyViewportEvent &event) { destroy_viewport(event.viewport_id); },
                     [&](const UpdateRenderObjectTransformEvent &event)
                     {
                         const auto render_object =
                             render_objects_.get(event.render_object_id)
                                 .map([](RenderObjectHandle &handle) -> RenderObject & { return *handle; });
                         if (render_object.has_value())
                         {
                             render_object->set_transform(event.transform);
                         }
                     }};

        while (!render_objects_.empty())
        {
            std::visit(callback, events_.front());
            events_.pop();
        }
    }
} // namespace retro
