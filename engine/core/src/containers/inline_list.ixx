/**
 * @file inline_list.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:inline_list;

import :defines;
import std;
import :concepts;

namespace retro
{
    template <typename T, usize N>
    struct InlineListStorage // NOLINT We want the buffer uninitialized
    {
        T *storage_data() noexcept
        {
            if (size_ == 0)
                return nullptr;

            return reinterpret_cast<T *>(data_);
        }
        const T *storage_data() const noexcept
        {
            if (size_ == 0)
                return nullptr;

            return reinterpret_cast<const T *>(data_);
        }

        [[nodiscard]] usize storage_size() const noexcept
        {
            return size_;
        }

        constexpr void unsafe_set_size(usize new_size) noexcept
        {
            size_ = static_cast<Size>(new_size);
        }

        static void unsafe_destroy(T *first, T *last) noexcept
        {
            for (auto *elem : std::span<T *>{first, last})
            {
                std::destroy_at(elem);
            }
        }

      private:
        using Size = SmallestSize<N>;
        alignas(T) std::byte data_[sizeof(T) * N]; // NOLINT We want this data uninitialized
        Size size_ = 0;
    };

    template <typename T>
    struct InlineListStorage<T, 0>
    {
        static constexpr T *storage_data() noexcept
        {
            return nullptr;
        }
        static constexpr usize storage_size() noexcept
        {
            return 0;
        }

        static constexpr void unsafe_set_size(uint8) noexcept
        {
        }

        static constexpr void unsafe_destroy(T *, T *) noexcept
        {
            // No-op
        }
    };

    template <typename T, usize N>
        requires(N > 0 && FullyTrivial<T>)
    struct InlineListStorage<T, N>
    {
        constexpr T *storage_data() noexcept
        {
            return data_.data();
        }
        constexpr const T *storage_data() const noexcept
        {
            return data_.data();
        }

        [[nodiscard]] constexpr usize storage_size() const noexcept
        {
            return size_;
        }

        constexpr void unsafe_set_size(usize new_size) noexcept
        {
            size_ = static_cast<Size>(new_size);
        }

        static constexpr void unsafe_destroy(T *, T *) noexcept
        {
            // No-op
        }

      private:
        using Size = SmallestSize<N>;
        alignas(alignof(T)) std::array<std::remove_const_t<T>, N> data_{};
        Size size_ = 0;
    };

    export template <typename T, usize N>
    class InlineList
    {
      public:
        using value_type = T;
        using size_type = usize;
        using difference_type = isize;
        using reference = value_type &;
        using const_reference = const value_type &;
        using pointer = value_type *;
        using const_pointer = const value_type *;
        using iterator = pointer;
        using const_iterator = const_pointer;
        using reverse_iterator = std::reverse_iterator<iterator>;
        using const_reverse_iterator = std::reverse_iterator<const_iterator>;

        constexpr InlineList() noexcept = default;
        constexpr explicit InlineList(const usize count)
            requires(std::is_default_constructible_v<T>)
        {
            resize(count);
        }
        constexpr InlineList(const usize count, const T &value)
        {
            resize(count, value);
        }

        template <std::input_iterator InputIt>
            requires std::convertible_to<std::iter_reference_t<T>, T>
        constexpr InlineList(InputIt first, InputIt last)
        {
            insert(begin(), first, last);
        }

        template <ContainerCompatibleRange<T> R>
        constexpr InlineList(std::from_range_t, R &&range)
        {
            insert_range(begin(), std::forward<R>(range));
        }

        constexpr InlineList(const InlineList &other)
            requires(N > 0 && !std::is_trivially_copy_constructible_v<T>)
        {
            for (auto &&e : other)
            {
                unchecked_emplace_back(e);
            }
        }

        constexpr InlineList(const InlineList &other)
            requires(N == 0 || std::is_trivially_copy_constructible_v<T>)
        = default;

        constexpr InlineList(InlineList &&other) noexcept(std::is_nothrow_move_constructible_v<T>)
            requires(N > 0 && !std::is_trivially_move_constructible_v<T>)
        {
            for (auto &&e : other)
            {
                unchecked_emplace_back(std::move(e));
            }
        }

        constexpr InlineList(InlineList &&other) noexcept(N == 0 || std::is_nothrow_move_constructible_v<T>)
            requires(N == 0 || std::is_trivially_move_constructible_v<T>)
        = default;

        constexpr explicit(false) InlineList(std::initializer_list<T> init)
            requires(std::constructible_from<T, std::ranges::range_reference_t<std::initializer_list<T>>> &&
                     std::movable<T>)
        {
            assign_range(init);
        }

        constexpr ~InlineList()
            requires(N > 0 && !std::is_trivially_destructible_v<T>)
        {
            storage_.unsafe_destroy(begin(), end());
        }
        constexpr ~InlineList()
            requires(N == 0 || std::is_trivially_destructible_v<T>)
        = default;

        constexpr InlineList &operator=(const InlineList &other)
            requires(N > 0 && (!std::is_trivially_copy_constructible_v<T> || !std::is_trivially_copy_assignable_v<T>))
        {
            clear();
            for (auto &&e : other)
            {
                unchecked_emplace_back(e);
            }
            return *this;
        }
        constexpr InlineList &operator=(const InlineList &other)
            requires(N == 0 || (std::is_trivially_copy_constructible_v<T> && std::is_trivially_copy_assignable_v<T>))
        = default;

        constexpr InlineList &operator=(InlineList &&other) noexcept(N == 0 ||
                                                                     (std::is_nothrow_move_constructible_v<T> &&
                                                                      std::is_nothrow_move_assignable_v<T>))
            requires(N > 0 && (!std::is_trivially_move_constructible_v<T> || !std::is_trivially_move_assignable_v<T>))
        {
            clear();
            for (auto &&e : other)
            {
                unchecked_emplace_back(std::move(e));
            }
            return *this;
        }
        constexpr InlineList &operator=(InlineList &&other) noexcept(N == 0 ||
                                                                     (std::is_nothrow_move_constructible_v<T> &&
                                                                      std::is_nothrow_move_assignable_v<T>))
            requires(N == 0 || (std::is_trivially_move_constructible_v<T> && std::is_trivially_move_assignable_v<T>))
        = default;

        constexpr InlineList &operator=(std::initializer_list<T> init)
            requires(std::constructible_from<T, std::ranges::range_reference_t<std::initializer_list<T>>> &&
                     std::movable<T>)
        {
            assign_range(init);
            return *this;
        }

        constexpr void assign(const usize count, const T &value)
        {
            clear();
            resize(count, value);
        }

        template <std::input_iterator InputIt>
            requires std::convertible_to<std::iter_reference_t<T>, T> && std::movable<T>
        constexpr void assign(InputIt first, InputIt last)
        {
            clear();
            insert(begin(), first, last);
        }

        constexpr void assign(std::initializer_list<T> init)
        {
            clear();
            insert(begin(), init);
        }

        template <ContainerCompatibleRange<T> R>
        constexpr void assign_range(R &&range)
        {
            clear();
            insert_range(begin(), std::forward<R>(range));
        }

        constexpr T &at(usize pos)
        {
            if (pos >= size()) [[unlikely]]
            {
                throw std::out_of_range{"InlineList::at"};
            } // NOLINT

            return storage_.storage_data()[pos];
        }
        constexpr const T &at(usize pos) const
        {
            if (pos >= size()) [[unlikely]]
            {
                throw std::out_of_range{"InlineList::at"};
            } // NOLINT

            return storage_.storage_data()[pos];
        }

        constexpr T &operator[](usize pos)
        {
            return storage_.storage_data()[pos];
        }

        constexpr const T &operator[](usize pos) const
        {
            return storage_.storage_data()[pos];
        }

        constexpr T &front()
        {
            return storage_.storage_data()[0];
        }
        constexpr const T &front() const
        {
            return storage_.storage_data()[0];
        }

        constexpr T &back()
        {
            return storage_.storage_data()[size() - 1];
        }

        constexpr const T &back() const
        {
            return storage_.storage_data()[size() - 1];
        }

        constexpr T *data()
        {
            return storage_.storage_data();
        }
        constexpr const T *data() const
        {
            return storage_.storage_data();
        }

        constexpr iterator begin()
        {
            return storage_.storage_data();
        }
        constexpr const_iterator begin() const
        {
            return storage_.storage_data();
        }
        constexpr const_iterator cbegin() const
        {
            return storage_.storage_data();
        }

        constexpr iterator end()
        {
            return std::next(storage_.storage_data(), storage_.storage_size());
        }
        constexpr const_iterator end() const
        {
            return std::next(storage_.storage_data(), storage_.storage_size());
        }

        constexpr const_iterator cend() const
        {
            return std::next(storage_.storage_data(), storage_.storage_size());
        }

        constexpr reverse_iterator rbegin()
        {
            return reverse_iterator{end()};
        }
        constexpr const_reverse_iterator rbegin() const
        {
            return const_reverse_iterator{end()};
        }
        constexpr const_reverse_iterator crbegin() const
        {
            return const_reverse_iterator{end()};
        }

        constexpr reverse_iterator rend()
        {
            return reverse_iterator{begin()};
        }
        constexpr const_reverse_iterator rend() const
        {
            return const_reverse_iterator{begin()};
        }
        constexpr const_reverse_iterator crend() const
        {
            return const_reverse_iterator{begin()};
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return size() == 0;
        }

        [[nodiscard]] constexpr usize size() const noexcept
        {
            return storage_.storage_size();
        }

        static constexpr usize max_size() noexcept
        {
            return N;
        }

        static constexpr usize capacity() noexcept
        {
            return N;
        }

        constexpr void resize(usize count)
            requires std::constructible_from<T, T &&> && std::default_initializable<T>
        {
            if (count == size())
                return;

            if (size > N) [[unlikely]]
            {
                throw std::bad_alloc{}; // NOLINT
            }

            if (count > size())
            {
                while (size() != count)
                {
                    emplace_back();
                }
            }
            else
            {
                unsafe_destroy(std::next(begin(), count), end());
                storage_.unsafe_set_size(count);
            }
        }
        constexpr void resize(usize count, const T &value)
            requires(std::constructible_from<T, const T &> && std::copyable<T>)
        {
            if (count == size())
                return;

            if (size > N) [[unlikely]]
            {
                throw std::bad_alloc{}; // NOLINT
            }

            if (count > size())
            {
                insert(end(), count - size(), value);
            }
            else
            {
                storage_.unsafe_destroy(std::next(begin(), count), end());
                storage_.unsafe_set_size(count);
            }
        }
        static constexpr void reserve(usize new_capacity)
        {
            if (new_capacity > capacity()) [[unlikely]]
            {
                throw std::bad_alloc{}; // NOLINT
            }
        }
        static constexpr void shrink_to_fit() noexcept
        {
            // This function exists for compatibility with vector-like interfaces.
        }

        constexpr iterator insert(const const_iterator pos, const T &value)
            requires std::constructible_from<T, const T &> && std::copyable<T>
        {
            return insert(pos, 1, value);
        }

        constexpr iterator insert(const_iterator pos, T &&value)
            requires std::constructible_from<T, T &&> && std::movable<T>
        {
            return emplace(pos, std::move(value));
        }

        constexpr iterator insert(const_iterator pos, const usize count, const T &value)
            requires std::constructible_from<T, const T &> && std::copyable<T>
        {
            auto last = end();
            for (usize i = 0; i < count; ++i)
            {
                emplace_back(value);
            }

            auto position = begin() + (pos - begin());
            std::rotate(position, last, end());
            return position;
        }

        template <typename InputIt>
        constexpr iterator insert(const_iterator pos, InputIt first, InputIt last)
        {
            if constexpr (std::random_access_iterator<InputIt>)
            {
                if (size() + static_cast<usize>(std::distance(first, last)) > capacity()) [[unlikely]]
                {
                    throw std::bad_alloc{}; // NOLINT
                }
            }

            auto original_end = end();
            for (; first != last; ++first)
            {
                emplace_back(*first);
            }

            auto position = begin() + (pos - begin());
            std::rotate(position, original_end, end());
            return position;
        }

        constexpr iterator insert(const_iterator pos, std::initializer_list<T> ilist)
            requires(std::constructible_from<T, std::ranges::range_reference_t<std::initializer_list<T>>> &&
                     std::movable<T>)
        {
            return insert_range(pos, ilist);
        }

        template <ContainerCompatibleRange<T> R>
            requires(std::constructible_from<T, std::ranges::range_reference_t<std::initializer_list<T>>> &&
                     std::movable<T>)
        constexpr iterator insert_range(const const_iterator pos, R &&range)
        {
            return insert(pos, std::begin(range), std::end(range));
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr iterator emplace(const_iterator position, Args &&...args)
        {
            auto original_end = end();
            emplace_back(std::forward<Args>(args)...);
            auto pos = begin() + (position - begin());
            std::rotate(pos, original_end, end());
            return original_end;
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr T &emplace_back(Args &&...args)
        {
            auto *emplaced = try_emplace_back(std::forward<Args>(args)...);
            if (emplaced == nullptr) [[unlikely]]
            {
                throw std::bad_alloc{}; // NOLINT
            }

            return *emplaced;
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr T *try_emplace_back(Args &&...args)
        {
            if (size() == capacity()) [[unlikely]]
            {
                return nullptr;
            }

            return std::addressof(unchecked_emplace_back(std::forward<Args>(args)...));
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr T &unchecked_emplace_back(Args &&...args)
        {
            std::construct_at(std::next(storage_.storage_data(), size()), std::forward<Args>(args)...);
            storage_.unsafe_set_size(size() + 1);
            return back();
        }

        constexpr T &push_back(const T &value)
            requires(std::copy_constructible<T>)
        {
            return emplace_back(value);
        }

        constexpr T &push_back(T &&value)
            requires(std::move_constructible<T>)
        {
            return emplace_back(std::move(value));
        }

        constexpr T *try_push_back(const T &value)
            requires(std::copy_constructible<T>)
        {
            return try_emplace_back(value);
        }

        constexpr T *try_push_back(T &&value)
            requires(std::move_constructible<T>)
        {
            return try_emplace_back(std::move(value));
        }

        constexpr T &unchecked_push_back(const T &value)
            requires(std::copy_constructible<T>)
        {
            return unchecked_emplace_back(value);
        }

        constexpr T &unchecked_push_back(T &&value)
            requires(std::move_constructible<T>)
        {
            return unchecked_emplace_back(std::move(value));
        }

        constexpr void pop_back()
        {
            if (!empty())
            {
                storage_.unsafe_destroy(std::prev(end()), end());
                storage_.unsafe_set_size(size() - 1);
            }
        }

        template <ContainerCompatibleRange<T> R>
            requires std::constructible_from<T, std::ranges::range_reference_t<R>>
        constexpr void append_range(R &&range)
        {
            if constexpr (std::ranges::sized_range<R>)
            {
                if (size() + std::ranges::size(range) > capacity()) [[unlikely]]
                {
                    throw std::bad_alloc{}; // NOLINT
                }
            }

            for (auto &&e : std::forward<R>(range))
            {
                if (size() == capacity()) [[unlikely]]
                {
                    throw std::bad_alloc{}; // NOLINT
                }

                emplace_back(std::forward<decltype(e)>(e));
            }
        }

        template <ContainerCompatibleRange<T> R>
            requires std::constructible_from<T, std::ranges::range_reference_t<R>>
        constexpr std::ranges::borrowed_iterator_t<R> try_append_range(R &&range)
        {
            auto it = std::ranges::begin(range);
            const auto end = std::ranges::end(range);
            for (; size != capacity() && it != end; ++it)
            {
                unchecked_emplace_back(*it);
            }

            return it;
        }

        constexpr void clear() noexcept
        {
            storage_.unsafe_destroy(begin(), end());
            storage_.unsafe_set_size(0);
        }

        constexpr iterator erase(const_iterator position)
            requires(std::movable<T>)
        {
            return erase(position, std::next(position));
        }

        constexpr iterator erase(const_iterator first, const_iterator last)
            requires(std::movable<T>)
        {
            auto start = begin() + (first - begin());
            if (first != last)
            {
                storage_.unsafe_destroy(std::move(start + (last - first), end(), start), end());
                storage_.unsafe_set_size(size() - static_cast<usize>(std::distance(first, last)));
            }
            return start;
        }

        constexpr void swap(InlineList &other) noexcept(N == 0 || (std::is_nothrow_swappable_v<T> &&
                                                                   std::is_nothrow_move_constructible_v<T>))
            requires std::movable<T>
        {
            auto tmp = std::move(other);
            other = std::move(*this);
            *this = std::move(tmp);
        }

        friend void swap(InlineList &lhs, InlineList &rhs) noexcept(N == 0 || (std::is_nothrow_swappable_v<T> &&
                                                                               std::is_nothrow_move_constructible_v<T>))
        {
            lhs.swap(rhs);
        }

        template <typename U = T>
        friend constexpr usize erase(InlineList &list, const U &value)
        {
            auto it = std::remove(list.begin(), list.end(), value);
            auto r = std::distance(it, list.end());
            list.erase(it, list.end());
            return r;
        }

        template <typename Pred>
        friend constexpr usize erase_if(InlineList &list, Pred pred)
        {
            auto it = std::remove_if(list.begin(), list.end(), pred);
            auto r = std::distance(it, list.end());
            list.erase(it, list.end());
            return r;
        }

        constexpr friend bool operator==(const InlineList &lhs, const InlineList &rhs)
            requires(std::equality_comparable<T>)
        {
            return lhs.size() == rhs.size() && std::ranges::equal(lhs, rhs);
        }

        constexpr friend auto operator<=>(const InlineList &lhs, const InlineList &rhs) noexcept
            requires LessThanComparableWith<T, T>
        {
            if constexpr (std::three_way_comparable<T>)
            {
                return std::lexicographical_compare_three_way(lhs.begin(), lhs.end(), rhs.begin(), rhs.end());
            }
            else
            {
                const auto sz = std::min(lhs.size(), rhs.size());
                for (usize i = 0; i < sz; ++i)
                {
                    if (lhs[i] < rhs[i])
                    {
                        return std::strong_ordering::less;
                    }

                    if (rhs[i] < lhs[i])
                    {
                        return std::strong_ordering::greater;
                    }
                }

                return lhs.size() <=> rhs.size();
            }
        }

      private:
        InlineListStorage<T, N> storage_;
    };
} // namespace retro
