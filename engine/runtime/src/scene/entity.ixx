//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.entity;

import std;
import :scene.component;
import :scene.transform;

namespace retro
{
    export class RETRO_API Entity
    {
      public:
        Entity() = default;
        ~Entity() = default;

        Entity(const Entity &) = delete;
        Entity(Entity &&) = default;
        Entity &operator=(const Entity &) = delete;
        Entity &operator=(Entity &&) = default;

        Transform &transform() noexcept
        {
            return transform_;
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

      private:
        Transform transform_;
        std::vector<std::unique_ptr<Component>> components;
    };
} // namespace retro