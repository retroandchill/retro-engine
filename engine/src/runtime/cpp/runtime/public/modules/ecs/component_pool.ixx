/**
 * @file component_pool.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.ecs.component_pool;

import std;
import retro.core.containers.optional;
import retro.runtime.ecs.entity;
import retro.core.util.noncopyable;
import retro.core.type_traits.range;

namespace retro
{
    export class ComponentPool : NonCopyable
    {
      public:
        virtual ~ComponentPool() = default;

        virtual bool erase(Entity entity) = 0;

        [[nodiscard]] virtual bool contains(Entity entity) const noexcept = 0;

        virtual void clear() noexcept = 0;

        [[nodiscard]] virtual std::span<const Entity> entities() const noexcept = 0;
    };

    export template <std::movable T>
    class ComponentPoolImpl final : public ComponentPool
    {
      public:
        template <typename... Args>
            requires std::constructible_from<T, Args...>
        T &emplace(const Entity entity, Args &&...args)
        {
            if (entity.index >= sparse_.size())
            {
                sparse_.resize(entity.index + 1, index_none<std::uint32_t>);
            }

            if (contains(entity))
            {
                auto &target = components_[sparse_[entity.index]];
                target = T{std::forward<Args>(args)...};
                return target;
            }

            const auto dense_index = static_cast<std::uint32_t>(components_.size());
            sparse_[entity.index] = dense_index;
            entities_.push_back(entity);
            return components_.emplace_back(std::forward<Args>(args)...);
        }

        bool erase(const Entity entity) override
        {
            if (!contains(entity))
            {
                return false;
            }

            const auto dense_index = sparse_[entity.index];
            const auto last_index = static_cast<std::uint32_t>(components_.size() - 1);
            if (dense_index != last_index)
            {
                components_[dense_index] = std::move(components_[last_index]);
                entities_[dense_index] = entities_[last_index];
                sparse_[entities_[dense_index].index] = dense_index;
            }

            components_.pop_back();
            entities_.pop_back();
            sparse_[entity.index] = index_none<std::uint32_t>;
            return true;
        }

        [[nodiscard]] bool contains(const Entity entity) const noexcept override
        {
            if (entity.index >= sparse_.size())
                return false;

            const auto dense_index = sparse_[entity.index];
            return dense_index != index_none<std::uint32_t> && dense_index < entities_.size() &&
                   entities_[dense_index] == entity;
        }

        Optional<T &> try_get(const Entity entity) noexcept
        {
            if (!contains(entity))
            {
                return std::nullopt;
            }

            return components_[sparse_[entity.index]];
        }

        Optional<const T &> try_get(const Entity entity) const noexcept
        {
            if (!contains(entity))
            {
                return std::nullopt;
            }

            return components_[sparse_[entity.index]];
        }

        void clear() noexcept override
        {
            components_.clear();
            entities_.clear();
            sparse_.clear();
        }

        std::span<T> components() noexcept
        {
            return components_;
        }

        std::span<const T> components() const noexcept
        {
            return components_;
        }

        [[nodiscard]] std::span<const Entity> entities() const noexcept override
        {
            return entities_;
        }

      private:
        std::vector<T> components_;
        std::vector<Entity> entities_;
        std::vector<std::uint32_t> sparse_;
    };
} // namespace retro
