/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime;

import retro.core;

namespace retro
{
    Scene::Scene(PipelineManager &pipeline_manager) : pipeline_manager_{&pipeline_manager}
    {
    }

    void Scene::destroy_node(SceneNode &node)
    {
        detach_from_parent(&node);

        unindex_node(&node);

        assert(node.master_index_ < storage_.size());
        auto &existing = storage_[node.master_index_];
        auto &back = storage_.back();
        std::swap(existing, back);
        back->master_index_ = node.master_index_;
        storage_.pop_back();
    }

    void Scene::attach_to_parent(SceneNode *child, SceneNode *parent)
    {
        if (child == nullptr)
            return;

        detach_from_parent(child);

        child->parent_ = parent;
        if (parent != nullptr)
        {
            parent->children_.push_back(child);
        }

        child->transform_.dirty_ = true;
    }

    void Scene::detach_from_parent(SceneNode *node)
    {
        if (node == nullptr || node->parent_ == nullptr)
        {
            return;
        }

        auto *parent = node->parent_;
        auto &siblings = parent->children_;

        const auto it = std::ranges::find(siblings, node);
        if (it != siblings.end())
        {
            *it = siblings.back();
            siblings.pop_back();
        }
        else
        {
            assert(false && "Node was not found in parent's child list");
        }

        node->parent_ = nullptr;
        node->transform_.dirty_ = true;
    }

    void Scene::update_transforms()
    {
        for (auto *viewport : nodes_of_type<ViewportNode>())
        {
            update_transform(*viewport, Matrix3x3f::identity(), false);
        }
    }

    void Scene::collect_draw_calls(const Vector2u viewport_size)
    {
        pipeline_manager_->collect_all_draw_calls(*this, viewport_size);
    }

    std::span<SceneNode *const> Scene::nodes_of_type(const std::type_index type) const noexcept
    {
        const auto it = nodes_by_type_.find(type);
        if (it == nodes_by_type_.end())
        {
            return {};
        }

        return it->second;
    }

    void Scene::index_node(SceneNode *node)
    {
        auto &nodes_list = nodes_by_type_[std::type_index{typeid(*node)}];
        nodes_list.push_back(node);
        node->internal_index_ = nodes_list.size() - 1;
    }

    void Scene::unindex_node(SceneNode *node)
    {
        const auto it = nodes_by_type_.find(std::type_index{typeid(*node)});
        if (it == nodes_by_type_.end())
        {
            return;
        }

        auto &vec = it->second;
        assert(node->internal_index_ < vec.size());
        auto &current = vec[node->internal_index_];
        auto &back = vec.back();
        std::swap(current, back);
        back->internal_index_ = node->internal_index_;
        vec.pop_back();
        node->internal_index_ = std::dynamic_extent;
    }

    void Scene::update_transform(SceneNode &node, const Matrix3x3f &parent_world, const bool parent_changed)
    {
        auto &transform = node.transform();

        const bool should_update = transform.dirty_ || parent_changed;
        if (should_update)
        {
            transform.world_matrix_ = parent_world * transform.local_matrix();
            transform.dirty_ = false;
        }

        for (auto *child : node.children_)
        {
            update_transform(*child, transform.world_matrix_, should_update);
        }
    }
} // namespace retro
