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

#include <cassert>

module retro.runtime;

import retro.core;

namespace retro
{
    Scene::Scene(PipelineManager &pipeline_manager) : pipeline_manager_{&pipeline_manager}
    {
        RenderTypeRegistry::instance().register_listeners(registry_, *pipeline_manager_);
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
        detach_from_parent(entity);
        registry_.destroy(entity);
    }

    void Scene::attach_to_parent(entt::entity child, entt::entity parent)
    {
        detach_from_parent(child);

        auto &child_h = registry_.get<Hierarchy>(child);
        child_h.parent = parent;

        if (parent != entt::null)
        {
            auto &parent_h = registry_.get<Hierarchy>(parent);

            // Insert at the beginning of the children list (simplest approach)
            child_h.next_sibling = parent_h.first_child;
            parent_h.first_child = child;
        }

        // Mark the child as dirty so its world matrix is updated
        registry_.get<Transform>(child).dirty_ = true;
    }

    void Scene::detach_from_parent(const entt::entity entity)
    {
        auto &h = registry_.get<Hierarchy>(entity);
        if (h.parent == entt::null)
            return;

        // If it's the first child, just move the pointer
        if (auto &parent_h = registry_.get<Hierarchy>(h.parent); parent_h.first_child == entity)
        {
            parent_h.first_child = h.next_sibling;
        }
        else
        {
            // Otherwise, find the previous sibling
            auto prev = parent_h.first_child;
            bool found = false;
            while (prev != entt::null)
            {
                auto &prev_h = registry_.get<Hierarchy>(prev);
                if (prev_h.next_sibling == entity)
                {
                    prev_h.next_sibling = h.next_sibling;
                    found = true;
                    break;
                }
                prev = prev_h.next_sibling;
            }

            assert(found && "Entity was not found in parent's child list");
        }

        h.parent = entt::null;
        h.next_sibling = entt::null;
        registry_.get<Transform>(entity).dirty_ = true;
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
        // It's probably fine to reset the arena here since we're not deallocating or allocating any memory just
        // sorta retaining memory to avoid allocations when processing this is probably fine.
        pipeline_manager_->reset_arena();
        pipeline_manager_->collect_all_draw_calls(registry_, viewport_size);
    }

    void Scene::update_transform(const entt::entity entity, const Matrix3x3f &parentWorld, const bool parent_changed)
    {
        auto &&[transform, hierarchy] = registry_.get<Transform, Hierarchy>(entity);

        const bool should_update = transform.dirty_ || parent_changed;
        if (should_update)
        {
            transform.world_matrix_ = parentWorld * transform.local_matrix();
            transform.dirty_ = false;
        }

        auto child = hierarchy.first_child;
        while (child != entt::null)
        {
            const auto &child_hierarchy = registry_.get<Hierarchy>(child);
            update_transform(child, transform.world_matrix_, should_update);
            child = child_hierarchy.next_sibling;
        }
    }
} // namespace retro
