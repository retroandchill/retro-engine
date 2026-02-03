/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.scene;

import std;
import retro.core.util.noncopyable;
import retro.core.math.transform;
import retro.core.math.vector;

namespace retro
{
    export class Scene;

    export struct Viewport
    {
        Vector2f view_size{};
    };

    export class RETRO_API SceneNode : NonCopyable
    {
      public:
        virtual ~SceneNode() = default;

        [[nodiscard]] SceneNode *parent() const noexcept;
        [[nodiscard]] std::span<SceneNode *const> children() const noexcept;

        [[nodiscard]] inline const Transform2f &transform() const noexcept
        {
            return transform_;
        }

        void set_transform(const Transform2f &transform);

        [[nodiscard]] inline const Transform2f &world_transform() const noexcept
        {
            return world_transform_;
        }

      protected:
        friend class Scene;

        explicit SceneNode(Scene &scene) : scene_{std::addressof(scene)}
        {
        }

      private:
        void update_world_transform();

        Scene *scene_{};
        std::size_t master_index_ = std::dynamic_extent;
        std::size_t internal_index_ = std::dynamic_extent;
        SceneNode *parent_ = nullptr;
        std::vector<SceneNode *> children_;
        Transform2f transform_{};
        Transform2f world_transform_{};
    };

    export class RETRO_API ViewportNode final : public SceneNode
    {
      public:
        explicit ViewportNode(Scene &scene, const Vector2f view_size) : SceneNode(scene), viewport_{view_size}
        {
        }

        [[nodiscard]] inline Viewport &viewport() noexcept
        {
            return viewport_;
        }

        [[nodiscard]] inline const Viewport &viewport() const noexcept
        {
            return viewport_;
        }

      private:
        Viewport viewport_{};
    };

    export class SceneDrawProxy
    {
      public:
        virtual ~SceneDrawProxy() = default;

        virtual void collect_all_draw_calls(Scene &scene, Vector2u viewport_size) = 0;
    };

    class RETRO_API Scene final : NonCopyable
    {
      public:
        explicit Scene(SceneDrawProxy &draw_proxy);

        template <std::derived_from<SceneNode> T, typename... Args>
            requires std::constructible_from<T, Scene &, Args...>
        T &create_node(SceneNode *parent, Args &&...args)
        {
            auto node = std::make_unique<T>(*this, std::forward<Args>(args)...);

            T &ref = *node;

            attach_to_parent(&ref, parent);
            index_node(&ref);

            node->master_index_ = storage_.size();
            storage_.emplace_back(std::move(node));
            return ref;
        }

        inline ViewportNode &create_viewport(const Vector2f view_size)
        {
            return create_node<ViewportNode>(nullptr, view_size);
        }

        void destroy_node(SceneNode &node);

        void attach_to_parent(SceneNode *child, SceneNode *parent);
        void detach_from_parent(SceneNode *node);

        void collect_draw_calls(Vector2u viewport_size);

        [[nodiscard]] std::span<SceneNode *const> nodes_of_type(std::type_index type) const noexcept;

        template <std::derived_from<SceneNode> T>
            requires(!std::is_abstract_v<T>)
        [[nodiscard]] std::span<T *const> nodes_of_type() const noexcept
        {
            auto of_types = nodes_of_type(std::type_index{typeid(T)});
            auto *cast_data = reinterpret_cast<T *const *>(of_types.data());
            return std::span{cast_data, of_types.size()};
        }

      private:
        void index_node(SceneNode *node);
        void unindex_node(SceneNode *node);

        std::vector<std::unique_ptr<SceneNode>> storage_;
        std::unordered_map<std::type_index, std::vector<SceneNode *>> nodes_by_type_{};

        SceneDrawProxy *draw_proxy_;
    };
} // namespace retro
