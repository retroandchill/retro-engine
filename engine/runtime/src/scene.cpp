/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime.scene;

namespace retro
{
    void SceneNode::set_transform(const Transform2f &transform)
    {
        transform_ = transform;
        update_world_transform();
    }

    void SceneNode::update_world_transform()
    {
        if (parent_ != nullptr)
        {
            world_transform_ = parent_->world_transform_.concatenate(transform_);
        }
        else
        {
            world_transform_ = transform_;
        }

        for (auto *child : children_)
        {
            child->world_transform_ = world_transform_.concatenate(child->transform_);
        }
    }

    Scene::Scene(SceneDrawProxy &draw_proxy) : draw_proxy_{&draw_proxy}
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

        child->update_world_transform();
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
        node->update_world_transform();
    }

    void Scene::collect_draw_calls(const Vector2u viewport_size)
    {
        draw_proxy_->collect_all_draw_calls(*this, viewport_size);
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
} // namespace retro
