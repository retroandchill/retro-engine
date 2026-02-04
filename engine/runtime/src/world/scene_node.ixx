/**
 * @file scene_node.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.world.scene_node;

import std;
import retro.core.util.noncopyable;
import retro.core.math.transform;
import retro.core.containers.node_list;

namespace retro
{
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

        void attach_to_parent(SceneNode *parent);
        void detach_from_parent();

      private:
        void update_world_transform();

        NodeHook hook_;
        SceneNode *parent_ = nullptr;
        std::vector<SceneNode *> children_;
        Transform2f transform_{};
        Transform2f world_transform_{};

        friend class NodeList<SceneNode, &SceneNode::hook_>;

      public:
        using List = NodeList<SceneNode, &SceneNode::hook_>;
    };

    export using SceneNodeList = SceneNode::List;
} // namespace retro
