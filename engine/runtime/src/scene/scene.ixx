/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene;

import std;
import retro.core;
import :scene.rendering;

namespace retro
{
    export class Scene;

    export struct Viewport
    {
        Vector2f view_size{};
    };

    export class Transform
    {
      public:
        [[nodiscard]] constexpr Vector2f position() const noexcept
        {
            return position_;
        }

        constexpr void set_position(const Vector2f positon) noexcept
        {
            position_ = positon;
            dirty_ = true;
        }

        [[nodiscard]] constexpr float rotation() const noexcept
        {
            return rotation_;
        }

        constexpr void set_rotation(const float rotation) noexcept
        {
            rotation_ = rotation;
        }

        [[nodiscard]] constexpr Vector2f scale() const noexcept
        {
            return scale_;
        }

        constexpr void set_scale(const Vector2f scale) noexcept
        {
            scale_ = scale;
        }

        [[nodiscard]] constexpr Matrix3x3f local_matrix() const noexcept
        {
            return create_translation(position_) * create_rotation(rotation_) * create_scale(scale_);
        }

        [[nodiscard]] constexpr const Matrix3x3f &world_matrix() const noexcept
        {
            return world_matrix_;
        }

      private:
        friend class Scene;

        Vector2f position_{};
        float rotation_{0};
        Vector2f scale_{1, 1};

        Matrix3x3f world_matrix_ = Matrix3x3f::identity();
        bool dirty_ = true;
    };

    export class RETRO_API SceneNode : NonCopyable
    {
      public:
        virtual ~SceneNode() = default;

        [[nodiscard]] SceneNode *parent() const noexcept;
        [[nodiscard]] std::span<SceneNode *const> children() const noexcept;

        [[nodiscard]] inline Transform &transform() noexcept
        {
            return transform_;
        }

        [[nodiscard]] inline const Transform &transform() const noexcept
        {
            return transform_;
        }

      protected:
        friend class Scene;

        explicit SceneNode(Scene &scene) : scene_{std::addressof(scene)}
        {
        }

      private:
        Scene *scene_{};
        SceneNode *parent_ = nullptr;
        std::vector<SceneNode *> children_;
        Transform transform_{};
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

    class RETRO_API Scene final : NonCopyable
    {
      public:
        explicit Scene(PipelineManager &pipeline_manager);

        template <std::derived_from<SceneNode> T, typename... Args>
            requires std::constructible_from<T, Scene &, Args...>
        T &create_node(SceneNode *parent, Args &&...args)
        {
            auto node = std::make_unique<T>(*this, std::forward<Args>(args)...);

            T &ref = *node;

            attach_to_parent(&ref, parent);
            index_node(&ref);

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

        void update_transforms();

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

        void update_transform(SceneNode &node, const Matrix3x3f &parent_world, bool parent_changed);

        std::vector<std::unique_ptr<SceneNode>> storage_;
        std::unordered_map<std::type_index, std::vector<SceneNode *>> nodes_by_type_{};

        PipelineManager *pipeline_manager_;
    };
} // namespace retro
