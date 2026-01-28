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
        RenderTypeRegistry::instance().register_listeners(*pipeline_manager_);
    }

    void Scene::destroy_node(SceneNode &node)
    {
        detach_from_parent(&node);

        unindex_node(&node);

        if (const auto it =
                std::ranges::find_if(storage_,
                                     [&node](const std::unique_ptr<SceneNode> &p) { return p.get() == &node; });
            it != storage_.end())
        {
            auto &back = storage_.back();
            std::swap(*it, back);
            storage_.pop_back();
        }
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
        nodes_by_type_[std::type_index{typeid(*node)}].push_back(node);
    }

    void Scene::unindex_node(SceneNode *node)
    {
        const auto key = std::type_index{typeid(*node)};
        const auto it = nodes_by_type_.find(key);
        if (it == nodes_by_type_.end())
        {
            return;
        }

        auto &vec = it->second;
        if (const auto found = std::ranges::find(vec, node); found != vec.end())
        {
            *found = vec.back();
            vec.pop_back();
        }
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
