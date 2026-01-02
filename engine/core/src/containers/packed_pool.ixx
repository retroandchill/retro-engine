//
// Created by fcors on 1/1/2026.
//

export module retro.core:containers.packed_pool;

import std;
import :defines;

namespace retro
{
    export template <typename Index = uint32, typename Generation = uint32>
        requires std::is_integral_v<Index> && std::is_integral_v<Generation>
    struct BasicHandle
    {
        using IndexType = Index;
        using GenerationType = Generation;

        Index index{0};
        Generation generation{0};

        friend constexpr auto operator==(const BasicHandle &lhs, const BasicHandle &rhs) noexcept
        {
            return lhs.index == rhs.index && lhs.generation == rhs.generation;
        }

        friend constexpr auto operator<=>(const BasicHandle &lhs, const BasicHandle &rhs) noexcept
        {
            auto index_cmp = lhs.index <=> rhs.index;
            if (index_cmp != std::strong_ordering::equal)
                return index_cmp;

            return lhs.generation <=> rhs.generation;
        }
    };

    export using DefaultHandle = BasicHandle<>;

    template <typename>
    struct IsBasicHandleSpecialization : std::false_type
    {
    };

    template <typename Index, typename Generation>
    struct IsBasicHandleSpecialization<BasicHandle<Index, Generation>> : std::true_type
    {
    };

    template <typename T>
    concept BasicHandleSpecialization = IsBasicHandleSpecialization<T>::value;

    template <typename Index = uint32, typename Generation = uint32>
        requires std::is_integral_v<Index> && std::is_integral_v<Generation>
    struct SlotEntry
    {
        Index dense_index{0};
        Index generation{0};
        bool alive{false};
    };

    export template <typename T>
    concept PackableType = requires(T value) {
        typename std::remove_cvref_t<T>::IdType;
        { value.id() } -> std::convertible_to<typename std::remove_cvref_t<T>::IdType>;
    } && BasicHandleSpecialization<typename std::remove_cvref_t<T>::IdType>;

    template <typename>
    struct PackableConstructionType
    {
        static constexpr bool is_valid = false;
    };

    template <PackableType T>
    struct PackableConstructionType<T>
    {
        static constexpr bool is_valid = true;

        static constexpr bool allow_polymorphic = false;

        using IdType = std::remove_cvref_t<T>::IdType;

        template <typename... Args>
            requires std::constructible_from<T, IdType, Args...>
        static constexpr T &emplace_info(std::vector<T> &values, IdType id, Args &&...args)
        {
            return values.emplace_back(id, std::forward<Args>(args)...);
        }

        static IdType get_id(const T &ptr)
        {
            return ptr.id();
        }
    };

    template <PackableType T>
    struct PackableConstructionType<std::unique_ptr<T>>
    {
        static constexpr bool is_valid = true;

        static constexpr bool allow_polymorphic = true;

        using IdType = PackableConstructionType<T>::IdType;

        template <typename... Args>
            requires std::constructible_from<T, IdType, Args...>
        static constexpr std::unique_ptr<T> &emplace_info(std::vector<std::unique_ptr<T>> &values,
                                                          IdType id,
                                                          Args &&...args)
        {
            return values.emplace_back(std::make_unique<T>(id, std::forward<Args>(args)...));
        }

        template <std::derived_from<T> U, typename... Args>
            requires std::constructible_from<U, IdType, Args...>
        static constexpr std::unique_ptr<T> &emplace_as(std::vector<std::unique_ptr<T>> &values,
                                                        IdType id,
                                                        Args &&...args)
        {
            return values.emplace_back(std::make_unique<U>(id, std::forward<Args>(args)...));
        }

        static IdType get_id(const std::unique_ptr<T> &ptr)
        {
            return ptr->id();
        }
    };

    template <typename T>
    concept Packable = PackableConstructionType<T>::is_valid;

    template <typename T, typename... Args>
    concept EmplaceablePackable = Packable<T> && requires(std::vector<T> &values, Args &&...args) {
        { PackableConstructionType<T>::emplace_info(values, std::forward<Args>(args)...) } -> std::same_as<T &>;
    };

    template <typename T, typename U, typename... Args>
    concept EmplaceablePackableAs =
        Packable<T> && PackableConstructionType<T>::allow_polymorphic &&
        requires(std::vector<T> &values, Args &&...args) {
            {
                PackableConstructionType<T>::template emplace_as<U>(values, std::forward<Args>(args)...)
            } -> std::same_as<T &>;
        };

    export template <Packable T>
    class PackedPool
    {
      public:
        using value_type = T;
        using HandleType = PackableConstructionType<T>::IdType;
        using IndexType = HandleType::IndexType;
        using GenerationType = HandleType::GenerationType;

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
            requires EmplaceablePackable<T, HandleType, Args...>
        constexpr T &emplace(Args &&...args)
        {
            return emplace_impl(
                [&](HandleType id) -> T &
                { return PackableConstructionType<T>::emplace_info(values_, id, std::forward<Args>(args)...); });
        }

        template <typename U, typename... Args>
            requires EmplaceablePackableAs<T, U, HandleType, Args...>
        constexpr T &emplace_as(Args &&...args)
        {
            return emplace_impl(
                [&](HandleType id) -> T &
                {
                    return PackableConstructionType<T>::template emplace_as<U>(values_,
                                                                               id,
                                                                               std::forward<Args>(args)...);
                });
        }

        constexpr void remove(HandleType id)
        {
            if (id.index >= slots_.size())
                return;

            auto &[dense_index, generation, alive] = slots_[id.index];
            if (!alive || generation != id.generation)
                return;

            if (const uint32 last_index = slots_.size() - 1; dense_index != last_index)
            {
                slots_[dense_index] = std::move(slots_[last_index]);

                const auto &moved = values_[dense_index];
                auto [index, generation] = PackableConstructionType<T>::get_id(moved);
                auto &moved_slot = slots_[index];
                moved_slot.dense_index = dense_index;
            }

            values_.pop_back();

            alive = false;
            ++generation;
            free_list_.push_back(id.index);
        }

      private:
        template <typename Functor>
            requires std::invocable<Functor, HandleType>
        constexpr T &emplace_impl(Functor &&functor)
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

            HandleType id{.index = slot_index, .generation = generation};
            auto &value = std::forward<Functor>(functor)(id);
            alive = true;

            return value;
        }

        using SlotType = SlotEntry<IndexType, GenerationType>;

        std::vector<T> values_{};
        std::vector<SlotType> slots_{};
        std::vector<IndexType> free_list_{};
    };
} // namespace retro