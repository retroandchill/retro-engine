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
    Scene::Scene(Renderer2D *renderer) : pipeline_manager_{renderer}
    {
        RenderTypeRegistry::instance().register_listeners(registry_, pipeline_manager_);
    }

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
        for (const auto view = registry_.view<Viewport>(); const auto entity : view)
        {
            update_transform(entity, Matrix3x3f::identity(), false);
        }
    }

    void Scene::collect_draw_calls(const Vector2u viewport_size)
    {
        pipeline_manager_.collect_all_draw_calls(registry_, viewport_size);
    }

    void Scene::update_transform(const entt::entity entity, const Matrix3x3f &parentWorld, const bool parent_changed)
    {
        auto &&[transform, hierarchy] = registry_.get<Transform, Hierarchy>(entity);

        const bool should_update = transform.dirty || parent_changed;
        if (should_update)
        {
            transform.world_matrix = parentWorld * transform.local_matrix();
            transform.dirty = false;
        }

        auto child = hierarchy.first_child;
        while (child != entt::null)
        {
            const auto &child_hierarchy = registry_.get<Hierarchy>(child);
            update_transform(child, transform.world_matrix, should_update);
            child = child_hierarchy.next_sibling;
        }
    }
} // namespace retro
