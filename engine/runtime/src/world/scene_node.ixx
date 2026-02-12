/**
 * @file scene_node.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.runtime.world.scene_node;

import std;
import retro.core.util.noncopyable;
import retro.core.math.transform;

namespace retro
{
    struct NodeHook
    {
        std::size_t master_index = std::dynamic_extent;
        std::size_t internal_index = std::dynamic_extent;
    };

    export class SceneNodeList;

    export class RETRO_API SceneNode : NonCopyable
    {
      public:
        virtual ~SceneNode() = default;

        [[nodiscard]] inline SceneNode *parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] inline std::span<SceneNode *const> children() const noexcept
        {
            return children_;
        }

        [[nodiscard]] inline const Transform2f &transform() const noexcept
        {
            return transform_;
        }

        void set_transform(const Transform2f &transform);

        [[nodiscard]] inline const Transform2f &world_transform() const noexcept
        {
            return world_transform_;
        }

        [[nodiscard]] inline std::int32_t z_order() const noexcept
        {
            return z_order_;
        }

        void set_z_order(const std::int32_t z_order);

        void attach_to_parent(SceneNode *parent);
        void detach_from_parent();

      private:
        void update_world_transform();

        friend class SceneNodeList;

        NodeHook hook_;
        SceneNode *parent_ = nullptr;
        std::vector<SceneNode *> children_;
        Transform2f transform_{};
        Transform2f world_transform_{};
        std::int32_t z_order_{0};
    };

    class RETRO_API SceneNodeList
    {
      public:
        SceneNodeList() = default;
        SceneNodeList(const SceneNodeList &) = delete;
        SceneNodeList(SceneNodeList &&) = default;
        ~SceneNodeList() = default;
        SceneNodeList &operator=(const SceneNodeList &) = delete;
        SceneNodeList &operator=(SceneNodeList &&) = default;

        [[nodiscard]] inline std::span<const std::unique_ptr<SceneNode>> nodes() const noexcept
        {
            return storage_;
        }

        [[nodiscard]] std::span<SceneNode *const> nodes_of_type(std::type_index type) const noexcept;

        template <std::derived_from<SceneNode> T>
        [[nodiscard]] std::span<T *const> nodes_of_type() const noexcept
        {
            auto of_types = nodes_of_type(std::type_index{typeid(T)});
            auto *cast_data = reinterpret_cast<T *const *>(of_types.data());
            return std::span{cast_data, of_types.size()};
        }

        void add(std::unique_ptr<SceneNode> node) noexcept;

        void remove(SceneNode &node) noexcept;

      private:
        void index_node(SceneNode *node) noexcept;

        void unindex_node(SceneNode *node);

        std::vector<std::unique_ptr<SceneNode>> storage_;
        std::unordered_map<std::type_index, std::vector<SceneNode *>> nodes_by_type_{};
    };
} // namespace retro
