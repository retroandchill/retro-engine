/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.world.scene;

import std;
import retro.core.util.noncopyable;
import retro.core.math.transform;
import retro.core.math.vector;
import retro.runtime.world.scene_node;

namespace retro
{
    export class RETRO_API Scene final : NonCopyable
    {
      public:
        template <std::derived_from<SceneNode> T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &create_node(SceneNode *parent, Args &&...args)
        {
            auto node = std::make_unique<T>(std::forward<Args>(args)...);

            T &ref = *node;

            ref.attach_to_parent(parent);
            nodes_.add(std::move(node));
            return ref;
        }

        void destroy_node(SceneNode &node);

        [[nodiscard]] inline const SceneNodeList &nodes() const noexcept
        {
            return nodes_;
        }

        [[nodiscard]] std::span<SceneNode *const> nodes_of_type(std::type_index type) const noexcept;

        template <std::derived_from<SceneNode> T>
            requires(!std::is_abstract_v<T>)
        [[nodiscard]] std::span<T *const> nodes_of_type() const noexcept
        {
            return nodes_.nodes_of_type<T>();
        }

      private:
        SceneNodeList nodes_;
    };
} // namespace retro
