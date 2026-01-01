//
// Created by fcors on 12/31/2025.
//
module;

#include <cstddef>

#include "retro/core/exports.h"

export module retro.runtime:scene.entity;

import std;
import :scene.component;
import :scene.transform;

namespace retro
{
    export struct EntityID
    {
        uint32 index{};
        uint32 generation{};
    };

    export class RETRO_API Entity
    {
      public:
        explicit inline Entity(const EntityID id, const Transform& transform) : id_{id}, transform_{transform} {}
        ~Entity() = default;

        Entity(const Entity &) = delete;
        Entity(Entity &&) = default;
        Entity &operator=(const Entity &) = delete;
        Entity &operator=(Entity &&) = default;

        [[nodiscard]] inline EntityID id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] inline const Transform &transform() const noexcept
        {
            return transform_;
        }

        inline void set_transform(const Transform &transform) noexcept
        {
            transform_ = transform;
        }

        [[nodiscard]] inline Vector2f position() const noexcept
        {
            return transform_.position;
        }

        inline void set_position(const Vector2f position) noexcept
        {
            transform_.position = position;
        }

        [[nodiscard]] inline float rotation() const noexcept
        {
            return transform_.rotation;
        }

        inline void set_rotation(const float rotation) noexcept
        {
            transform_.rotation = rotation;
        }

        [[nodiscard]] inline Vector2f scale() const noexcept
        {
            return transform_.scale;
        }

        inline void set_scale(const Vector2f scale) noexcept
        {
            transform_.scale = scale;
        }

        template <std::derived_from<Component> T, typename... Args>
            requires std::constructible_from<T, Entity &, Args...>
        T &create_component(Args &&...args)
        {
            std::unique_ptr<Component> &component =
                components.emplace_back(std::make_unique<T>(*this, std::forward<Args>(args)...));
            component->on_attach();
            return static_cast<T &>(*component);
        }

        constexpr static int32 transform_offset()
        {
            return offsetof(Entity, transform_);
        }

      private:
        EntityID id_;
        Transform transform_;
        std::vector<std::unique_ptr<Component>> components;
    };
} // namespace retro