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
} // namespace retro
