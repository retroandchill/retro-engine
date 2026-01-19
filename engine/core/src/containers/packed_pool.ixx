/**
 * @file packed_pool.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:containers.packed_pool;

import std;
import boost;
import :defines;
import :containers.polymorphic;

namespace retro
{
    export struct PoolHandle
    {
        static constexpr uint32 INVALID_GENERATION = 0;

        uint32 index{0};
        uint32 generation{0};

        static constexpr PoolHandle invalid() noexcept
        {
            return {0, 0};
        }

        [[nodiscard]] constexpr bool is_valid() const noexcept
        {
            return generation != 0;
        }

        friend constexpr auto operator==(const PoolHandle &lhs, const PoolHandle &rhs) noexcept
        {
            return lhs.index == rhs.index && lhs.generation == rhs.generation;
        }

        friend constexpr auto operator<=>(const PoolHandle &lhs, const PoolHandle &rhs) noexcept
        {
            auto index_cmp = lhs.index <=> rhs.index;
            if (index_cmp != std::strong_ordering::equal)
                return index_cmp;

            return lhs.generation <=> rhs.generation;
        }
    };

    struct SlotEntry
    {
        uint32 dense_index{0};
        uint32 generation{1};
        bool alive{false};
    };

    export template <typename T>
    class PackedPool
    {
      public:
        using value_type = T;

        PackedPool() = default;

        PackedPool(const PackedPool &)
            requires(!std::copy_constructible<T>)
        = delete;
        PackedPool(const PackedPool &)
            requires std::copy_constructible<T>
        = default;

        PackedPool(PackedPool &&) noexcept = default;

        ~PackedPool() noexcept = default;

        PackedPool &operator=(const PackedPool &)
            requires(!std::copy_constructible<T>)
        = delete;
        PackedPool &operator=(const PackedPool &)
            requires std::copy_constructible<T>
        = default;

        PackedPool &operator=(PackedPool &&) noexcept = default;

        [[nodiscard]] constexpr T &operator[](usize index) noexcept
        {
            return values_[index];
        }

        [[nodiscard]] constexpr const T &operator[](usize index) const noexcept
        {
            return values_[index];
        }

        [[nodiscard]] constexpr T &at(usize index) noexcept
        {
            return values_.at(index);
        }

        [[nodiscard]] constexpr const T &at(usize index) const noexcept
        {
            return values_.at(index);
        }

        [[nodiscard]] constexpr T *data() noexcept
        {
            return values_.data();
        }

        [[nodiscard]] constexpr const T *data() const noexcept
        {
            return values_.data();
        }

        [[nodiscard]] constexpr T &front() noexcept
        {
            return values_.front();
        }

        [[nodiscard]] constexpr const T &front() const noexcept
        {
            return values_.front();
        }

        [[nodiscard]] constexpr T &back() noexcept
        {
            return values_.back();
        }

        [[nodiscard]] constexpr const T &back() const noexcept
        {
            return values_.back();
        }

        [[nodiscard]] constexpr auto begin() noexcept
        {
            return values_.begin();
        }
        [[nodiscard]] constexpr auto begin() const noexcept
        {
            return values_.cbegin();
        }
        [[nodiscard]] constexpr auto end() noexcept
        {
            return values_.end();
        }
        [[nodiscard]] constexpr auto end() const noexcept
        {
            return values_.cend();
        }
        [[nodiscard]] constexpr auto cbegin() const noexcept
        {
            return values_.cbegin();
        }
        [[nodiscard]] constexpr auto cend() const noexcept
        {
            return values_.cend();
        }
        [[nodiscard]] constexpr auto rbegin() noexcept
        {
            return values_.rbegin();
        }
        [[nodiscard]] constexpr auto rbegin() const noexcept
        {
            return values_.rbegin();
        }
        [[nodiscard]] constexpr auto rend() noexcept
        {
            return values_.rend();
        }
        [[nodiscard]] constexpr auto rend() const noexcept
        {
            return values_.rend();
        }
        [[nodiscard]] constexpr auto crbegin() const noexcept
        {
            return values_.crbegin();
        }
        [[nodiscard]] constexpr auto crend() const noexcept
        {
            return values_.crend();
        }

        [[nodiscard]] constexpr usize size() const noexcept
        {
            return values_.size();
        }

        [[nodiscard]] constexpr usize max_size() const noexcept
        {
            return values_.max_size();
        }

        [[nodiscard]] constexpr usize capacity() const noexcept
        {
            return values_.capacity();
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return values_.empty();
        }

        template <typename Self>
        [[nodiscard]] constexpr auto get(this Self &self, const PoolHandle id) noexcept
            -> boost::optional<std::remove_cvref_t<T> &>
        {
            if (id.generation == 0 || id.index >= self.slots_.size())
                return boost::none;

            const auto &[dense_index, generation, alive] = self.slots_[id.index];
            if (!alive || generation != id.generation)
                return boost::none;

            return self.values_[dense_index];
        }

        constexpr void reserve(usize size)
        {
            values_.reserve(size);
            slots_.reserve(size);
        }

        constexpr void clear()
        {
            values_.clear();
            slots_.clear();
            free_list_.clear();
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr std::pair<PoolHandle, T &> emplace(Args &&...args)
        {
            uint32 slot_index;
            if (!free_list_.empty())
            {
                slot_index = free_list_.back();
                free_list_.pop_back();
            }
            else
            {
                slot_index = static_cast<uint32>(slots_.size());
                slots_.emplace_back();
            }

            auto &[dense_index, generation, alive] = slots_[slot_index];
            dense_index = static_cast<uint32>(values_.size());
            alive = true;

            T &value = values_.emplace_back(std::forward<Args>(args)...);
            reverse_map_.push_back(slot_index);

            return {{slot_index, generation}, value};
        }

        constexpr void remove(PoolHandle id)
        {
            if (id.index >= slots_.size())
                return;

            auto &[dense_index, generation, alive] = slots_[id.index];
            if (!alive || generation != id.generation)
                return;

            if (const uint32 last_dense_index = static_cast<uint32>(values_.size()) - 1;
                dense_index != last_dense_index)
            {
                // Move the last element into the hole
                values_[dense_index] = std::move(values_.back());

                // Update the slot of the element we just moved
                const uint32 moved_slot_index = reverse_map_.back();
                slots_[moved_slot_index].dense_index = dense_index;
                reverse_map_[dense_index] = moved_slot_index;
            }

            values_.pop_back();
            reverse_map_.pop_back();

            alive = false;
            ++generation;
            // Since this is only from the result of unsigned overflow this is very unlikely to ever happen.
            if (generation == PoolHandle::INVALID_GENERATION) [[unlikely]]
                generation = 1;
            free_list_.push_back(id.index);
        }

      private:
        std::vector<T> values_{};
        std::vector<uint32> reverse_map_{};
        std::vector<SlotEntry> slots_{};
        std::vector<uint32> free_list_{};
    };
} // namespace retro
