/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.world.scene;

namespace retro
{
    Entity Scene::create_entity()
    {
        const auto entity = entities_.create_entity();
        auto new_index = root_entities_.size();
        root_entities_.push_back(entity);
        root_entity_indices_.emplace(entity, new_index);
        return entity;
    }

    bool Scene::destroy_entity(const Entity entity)
    {
        if (!entities_.destroy_entity(entity))
            return false;

        auto it = root_entity_indices_.find(entity);
        if (it == root_entity_indices_.end())
            return true;

        if (it->second != root_entities_.size() - 1)
        {
            root_entities_[it->second] = root_entities_.back();
            root_entity_indices_[root_entities_.back()] = it->second;
        }

        root_entity_indices_.erase(it);
        root_entities_.pop_back();
        return true;
    }

    void Scene::attach_entity(Entity child, Entity parent)
    {
        auto &[current_parent, children] = entities_.get_or_add<HierarchyComponent>(child);
        if (current_parent != null_entity)
        {
            auto &[old_parent, old_children] = entities_.get<HierarchyComponent>(current_parent);
            std::ranges::remove(old_children, child);
        }
        else
        {
            const auto root_index = root_entity_indices_[child];
            root_entities_[root_index] = root_entities_.back();
            root_entity_indices_[root_entities_.back()] = root_index;
            root_entity_indices_.erase(child);
            root_entities_.pop_back();
        }

        current_parent = parent;
        if (current_parent != null_entity)
        {
            auto &[new_parent, new_children] = entities_.get<HierarchyComponent>(current_parent);
            new_children.push_back(child);
        }
        else
        {
            auto root_index = root_entities_.size();
            root_entities_.push_back(child);
            root_entity_indices_.emplace(child, root_index);
        }
    }

    bool Scene::detach_entity(Entity child)
    {
        auto hierarchy = entities_.try_get<HierarchyComponent>(child);
        if (!hierarchy.has_value())
            return false;

        if (hierarchy->parent != null_entity)
        {
            auto &[old_parent, old_children] = entities_.get<HierarchyComponent>(hierarchy->parent);
            std::ranges::remove(old_children, child);

            hierarchy->parent = null_entity;
            auto root_index = root_entities_.size();
            root_entities_.push_back(child);
            root_entity_indices_.emplace(child, root_index);
        }

        return true;
    }

    Scene &SceneManager::create_scene()
    {
        const auto &created_scene = scenes_.emplace_back(std::make_unique<Scene>());
        on_scene_created_(*created_scene);
        return *created_scene;
    }

    void SceneManager::destroy_scene(Scene &scene)
    {
        const auto it = std::ranges::find_if(scenes_,
                                             [&scene](const std::unique_ptr<Scene> &ptr)
                                             { return ptr.get() == std::addressof(scene); });

        if (it != scenes_.end())
        {
            on_scene_destroyed_(**it);
            scenes_.erase(it);
        }
    }
} // namespace retro
