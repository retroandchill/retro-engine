/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

// Workaround for IntelliSense issues regarding entt
#ifdef __JETBRAINS_IDE__
#include <entt/entt.hpp>
#endif

module retro.runtime;

import retro.core;
import entt;

namespace retro
{
    entt::entity Scene::create_entity()
    {
        const auto entity = registry_.create();
        registry_.emplace<Transform>(entity);
        registry_.emplace<Hierarchy>(entity);
        return entity;
    }

    entt::entity Scene::create_viewport(Vector2f view_size)
    {
        const auto entity = create_entity();
        registry_.emplace<Viewport>(entity, view_size);
        return entity;
    }

    void Scene::destroy_entity(const entt::entity entity)
    {
        registry_.destroy(entity);
    }

    void Scene::update_transforms()
    {
        for (auto &&[entity, viewport] : registry_.view<Viewport>().each())
        {
            update_transform(entity, create_translation(viewport.offset));
        }
    }

    void Scene::update_transform(entt::entity entity, const Matrix3x3f &parentWorld)
    {
        auto &&[transform, hierarchy] = registry_.get<Transform, Hierarchy>(entity);

        if (transform.dirty)
        {
            transform.world_matrix = parentWorld * transform.local_matrix();
            transform.dirty = false;
        }

        for (auto child = hierarchy.first_child; child != entt::null;
             child = registry_.get<Hierarchy>(child).next_sibling)
        {
            update_transform(child, transform.world_matrix);
        }
    }
} // namespace retro
