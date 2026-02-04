/**
 * @file scene_node.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime.world.scene_node;

namespace retro
{
    void SceneNode::set_transform(const Transform2f &transform)
    {
        transform_ = transform;
        update_world_transform();
    }

    void SceneNode::attach_to_parent(SceneNode *parent)
    {
        detach_from_parent();

        parent_ = parent;
        if (parent != nullptr)
        {
            parent->children_.push_back(this);
        }

        update_world_transform();
    }

    void SceneNode::detach_from_parent()
    {
        if (parent_ == nullptr)
        {
            return;
        }

        auto &siblings = parent_->children_;
        const auto it = std::ranges::find(siblings, this);
        if (it != siblings.end())
        {
            *it = siblings.back();
            siblings.pop_back();
        }
        else
        {
            assert(false && "Node was not found in parent's child list");
        }

        parent_ = nullptr;
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

    std::span<SceneNode *const> SceneNodeList::nodes_of_type(const std::type_index type) const noexcept
    {
        const auto it = nodes_by_type_.find(type);
        if (it == nodes_by_type_.end())
        {
            return {};
        }

        return it->second;
    }

    void SceneNodeList::add(std::unique_ptr<SceneNode> node) noexcept
    {
        index_node(node.get());
        node->hook_.master_index = storage_.size();
        storage_.emplace_back(std::move(node));
    }

    void SceneNodeList::remove(SceneNode &node) noexcept
    {
        unindex_node(&node);

        assert(node.hook_.master_index < storage_.size());
        auto &existing = storage_[node.hook_.master_index];
        auto &back = storage_.back();
        std::swap(existing, back);
        back->hook_.master_index = node.hook_.master_index;
        storage_.pop_back();
    }

    void SceneNodeList::index_node(SceneNode *node) noexcept
    {
        auto &nodes_list = nodes_by_type_[std::type_index{typeid(*node)}];
        nodes_list.push_back(node);
        node->hook_.internal_index = nodes_list.size() - 1;
    }

    void SceneNodeList::unindex_node(SceneNode *node)
    {
        const auto it = nodes_by_type_.find(std::type_index{typeid(*node)});
        if (it == nodes_by_type_.end())
        {
            return;
        }

        auto &vec = it->second;
        assert(node->hook_.internal_index < vec.size());
        auto &current = vec[node->hook_.internal_index];
        auto &back = vec.back();
        std::swap(current, back);
        back->hook_.internal_index = node->hook_.internal_index;
        vec.pop_back();
        node->hook_.internal_index = std::dynamic_extent;
    }
} // namespace retro
