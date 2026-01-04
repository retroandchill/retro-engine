/**
 * @file actor_ptr.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.actor_ptr;

import boost;
import std;
import retro.core;

namespace retro
{
    template <typename>
    struct ActorHandleResolver;

    template <typename T>
    concept ActorHandle = requires { typename ActorHandleResolver<T>::HandleType; } &&
                          requires(typename ActorHandleResolver<T>::HandleType handle) {
                              {
                                  ActorHandleResolver<T>::resolve(handle)
                              } -> std::same_as<boost::optional<T &>>;
                          };

    export template <ActorHandle T>
    class ActorPtr
    {
      public:
        using IdType = ActorHandleResolver<T>::HandleType;

        constexpr ActorPtr() noexcept = default;
        explicit(false) constexpr ActorPtr(std::nullptr_t) noexcept
        {
        }
        explicit(false) constexpr ActorPtr(const IdType id) : id_{id}
        {
        }
        explicit(false) constexpr ActorPtr(T &actor) noexcept : id_{actor.id()}
        {
        }
        explicit(false) constexpr ActorPtr(T *actor) noexcept : id_{actor != nullptr ? actor->id() : IdType{}}
        {
        }

        [[nodiscard]] constexpr bool is_valid() const noexcept
        {
            return try_get().has_value();
        }

        [[nodiscard]] T *operator->() const noexcept
        {
            auto opt = try_get();
            return opt ? std::addressof(*opt) : nullptr;
        }

        [[nodiscard]] T &operator*() const
        {
            return *try_get();
        }

        [[nodiscard]] T &get() const
        {
            return try_get().value();
        }

        [[nodiscard]] boost::optional<T &> try_get() const noexcept
        {
            return ActorHandleResolver<T>::resolve(id_);
        }

        [[nodiscard]] constexpr IdType id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] friend bool operator==(ActorPtr lhs, ActorPtr rhs) noexcept = default;

        [[nodiscard]] friend bool operator==(const ActorPtr lhs, std::nullptr_t) noexcept
        {
            return !lhs.is_valid();
        }

        [[nodiscard]] friend auto operator<=>(const ActorPtr &lhs, const ActorPtr &rhs) noexcept = default;

      private:
        IdType id_{};
    };

    export using EntityID = DefaultHandle;
    export using ComponentID = DefaultHandle;

    template <>
    struct ActorHandleResolver<class Entity>
    {
        using HandleType = EntityID;

        RETRO_API static boost::optional<Entity &> resolve(EntityID id);
    };

    template <>
    struct ActorHandleResolver<class Component>
    {
        using HandleType = ComponentID;

        RETRO_API static boost::optional<Component &> resolve(ComponentID id);
    };

    template <std::derived_from<Component> T>
    struct ActorHandleResolver<T>
    {
        using HandleType = ComponentID;

        static boost::optional<T &> resolve(const ComponentID id)
        {
            return ActorHandleResolver<Component>::resolve(id).map([](Component &value) -> T &
                                                                   { return dynamic_cast<T &>(value); });
        }
    };
} // namespace retro
