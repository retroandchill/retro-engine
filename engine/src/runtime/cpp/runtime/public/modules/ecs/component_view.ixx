/**
 * @file component_view.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.ecs.component_view;

import std;
import retro.runtime.ecs.entity;
import retro.core.containers.optional;
import retro.runtime.ecs.component_pool;

namespace retro
{
    export template <typename Manager, typename Component>
    concept ComponentViewManager = requires(Manager &manager, Entity entity) {
        {
            manager.template try_get<Component>(entity)
        } -> std::convertible_to<Optional<Component &>>;
        {
            manager.template entities_with_component<Component>()
        } -> std::convertible_to<std::span<const Entity>>;
    };

    export template <typename Manager, std::movable... Components>
        requires(sizeof...(Components) > 0 && (ComponentViewManager<Manager, Components> && ...))
    class ComponentView;

    template <typename Manager, std::movable First, std::movable... Rest>
    class ComponentView<Manager, First, Rest...>
    {
      public:
        class Iterator
        {
          public:
            using difference_type = std::ptrdiff_t;
            using value_type = std::tuple<Entity, First &, Rest &...>;
            using iterator_category = std::input_iterator_tag;

            Iterator() noexcept = default;

            Iterator(ComponentView &view, std::size_t index) noexcept : view_{std::addressof(view)}, index_{index}
            {
                skip_invalid();
            }

            Iterator &operator++()
            {
                ++index_;
                skip_invalid();
                return *this;
            }

            void operator++(int)
            {
                ++*this;
            }

            [[nodiscard]] bool operator==(std::default_sentinel_t) const noexcept
            {
                return view_ == nullptr || index_ >= view_->entities_.size();
            }

            [[nodiscard]] auto operator*() const
            {
                const Entity entity = view_->entities_[index_];

                return std::tuple<Entity, First &, Rest &...>{entity,
                                                              *view_->manager_->template try_get<First>(entity),
                                                              *view_->manager_->template try_get<Rest>(entity)...};
            }

          private:
            void skip_invalid()
            {
                if (view_ == nullptr || view_->entities_.empty())
                {
                    return;
                }

                const auto entities = view_->entities_;

                while (index_ < entities.size())
                {

                    if (const Entity entity = entities[index_];
                        (view_->manager_->template try_get<Rest>(entity).has_value() && ...))
                    {
                        return;
                    }

                    ++index_;
                }
            }

            ComponentView *view_ = nullptr;
            std::size_t index_ = 0;
        };

        explicit ComponentView(Manager &manager) noexcept
            : manager_{std::addressof(manager)}, entities_{manager.template entities_with_component<First>()}
        {
        }

        [[nodiscard]] Iterator begin() noexcept
        {
            return Iterator{*this, 0};
        }

        [[nodiscard]] std::default_sentinel_t end() noexcept
        {
            return std::default_sentinel;
        }

      private:
        Manager *manager_;
        std::span<const Entity> entities_;
    };
} // namespace retro
