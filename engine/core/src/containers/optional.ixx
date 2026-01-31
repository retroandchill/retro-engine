/**
 * @file optional.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:optional;

import std;
import :concepts;

namespace retro
{
    export template <typename T>
    class Optional;

    template <typename T>
    constexpr bool IS_OPTIONAL_SPECIALIZATION = false;
    template <typename T>
    constexpr bool IS_OPTIONAL_SPECIALIZATION<Optional<T>> = true;
    template <typename T>
    constexpr bool IS_OPTIONAL_SPECIALIZATION<std::optional<T>> = true;

    template <typename T>
    concept OptionalSpecialization = IS_OPTIONAL_SPECIALIZATION<T>;

    template <typename T, typename Container>
    struct OptionalIterator
    {
        using value_type = std::remove_cv_t<T>;
        using difference_type = isize;

        using iterator_concept = std::contiguous_iterator_tag;

        constexpr OptionalIterator() noexcept = default;
        constexpr explicit OptionalIterator(T *ptr) noexcept : ptr_{ptr}
        {
        }

        template <typename U>
            requires(std::same_as<std::remove_cv_t<U>, std::remove_cv_t<T>> && std::is_const_v<T> &&
                     !std::is_const_v<U>)
        constexpr OptionalIterator(const OptionalIterator<U, Container> &other) noexcept : ptr_{other.ptr_}
        {
        }

        constexpr T &operator*() const noexcept
        {
            return *ptr_;
        }
        constexpr T *operator->() const noexcept
        {
            return ptr_;
        }

        constexpr T &operator[](isize n) const noexcept
        {
            return ptr_[n];
        }

        constexpr OptionalIterator &operator++() noexcept
        {
            ++ptr_;
            return *this;
        }
        constexpr OptionalIterator operator++(int) noexcept
        {
            auto tmp = *this;
            ++(*this);
            return tmp;
        }
        constexpr OptionalIterator &operator--() noexcept
        {
            --ptr_;
            return *this;
        }
        constexpr OptionalIterator operator--(int) noexcept
        {
            auto tmp = *this;
            --(*this);
            return tmp;
        }

        constexpr OptionalIterator &operator+=(difference_type n) noexcept
        {
            ptr_ += n;
            return *this;
        }
        constexpr OptionalIterator &operator-=(difference_type n) noexcept
        {
            ptr_ -= n;
            return *this;
        }

        friend constexpr OptionalIterator operator+(OptionalIterator it, difference_type n) noexcept
        {
            it += n;
            return it;
        }
        friend constexpr OptionalIterator operator+(difference_type n, OptionalIterator it) noexcept
        {
            it += n;
            return it;
        }
        friend constexpr OptionalIterator operator-(OptionalIterator it, difference_type n) noexcept
        {
            it -= n;
            return it;
        }
        friend constexpr difference_type operator-(OptionalIterator a, OptionalIterator b) noexcept
        {
            return a.ptr_ - b.ptr_;
        }

        friend constexpr bool operator==(OptionalIterator, OptionalIterator) noexcept = default;
        friend constexpr auto operator<=>(OptionalIterator, OptionalIterator) noexcept = default;

      private:
        template <typename, typename>
        friend struct OptionalIterator;

        T *ptr_ = nullptr;
    };

    template <typename T, typename U>
    concept NotConstructibleFromOptional =
        !std::is_constructible_v<T, Optional<U> &> && !std::is_constructible_v<T, const Optional<U> &> &&
        !std::is_constructible_v<T, Optional<U> &&> && !std::is_constructible_v<T, const Optional<U> &&> &&
        !std::is_convertible_v<Optional<U> &, T> && !std::is_convertible_v<const Optional<U> &, T> &&
        !std::is_convertible_v<Optional<U> &&, T> && !std::is_convertible_v<const Optional<U> &&, T>;

    template <typename T, typename U>
    concept NotAssignableFromOptional =
        NotConstructibleFromOptional<T, U> && !std::is_assignable_v<T &, Optional<U> &> &&
        !std::is_assignable_v<T &, const Optional<U> &> && !std::is_assignable_v<T &, Optional<U> &&> &&
        !std::is_assignable_v<T &, const Optional<U> &&>;

    struct FromFunctionTag
    {
    };

    template <typename T>
    class Optional
    {
      public:
        using value_type = T;
        using iterator = OptionalIterator<T, Optional>;
        using const_iterator = OptionalIterator<const T, Optional>;

        constexpr Optional() noexcept
        {
        }

        constexpr explicit(false) Optional(std::nullopt_t) noexcept
        {
        }

        constexpr Optional(const Optional &other)
            requires(std::is_copy_constructible_v<T> && !std::is_trivially_copy_constructible_v<T>)
            : has_value_{other.has_value_}
        {
            if (has_value_)
            {
                std::construct_at(&value_, other.value_);
            }
        }

        constexpr Optional(const Optional &other)
            requires std::is_trivially_copy_constructible_v<T>
        = default;

        constexpr Optional(const Optional &other)
            requires(!std::is_copy_constructible_v<T>)
        = delete;

        constexpr Optional(Optional &&other) noexcept(std::is_nothrow_move_constructible_v<T>)
            requires(std::is_move_constructible_v<T> && !std::is_trivially_move_constructible_v<T>)
            : has_value_{other.has_value_}
        {
            if (has_value_)
            {
                std::construct_at(&value_, std::move(other.value_));
            }
        }

        constexpr Optional(Optional &&other) noexcept(std::is_nothrow_move_constructible_v<T>)
            requires std::is_trivially_move_constructible_v<T>
        = default;

        constexpr Optional(Optional &&other) noexcept(std::is_nothrow_move_constructible_v<T>)
            requires(!std::is_move_constructible_v<T>)
        = delete;

        template <typename U>
            requires std::is_constructible_v<T, const U &> && !std::same_as<T, U> && NotConstructibleFromOptional<T, U>
        constexpr explicit(!std::is_convertible_v<const U &, T>) Optional(const Optional<U> &value)
            : has_value_{value.has_value()}
        {
            if (has_value_)
            {
                std::construct_at(std::addressof(value_), *value);
            }
        }

        template <typename U>
            requires std::is_constructible_v<T, U> && !std::same_as<T, U> && NotConstructibleFromOptional<T, U>
        constexpr explicit(!std::is_convertible_v<U, T>) Optional(Optional<U> && other) : has_value_{other.has_value()}
        {
            if (has_value_)
            {
                std::construct_at(std::addressof(value_), *std::move(other));
            }
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr explicit Optional(std::in_place_t, Args &&...args)
            : value_(std::forward<Args>(args)...), has_value_{true}
        {
        }

        template <typename U, typename... Args>
            requires std::constructible_from<T, std::initializer_list<U> &, Args...>
        constexpr explicit Optional(std::in_place_t, std::initializer_list<U> il, Args &&...args)
            : value_{il, std::forward<Args>(args)...}, has_value_{true}
        {
        }

        template <typename U>
            requires(std::constructible_from<T, U> && !std::same_as<std::remove_cvref_t<U>, Optional> &&
                     !std::same_as<std::remove_cvref_t<U>, std::in_place_t>)
        constexpr explicit(!std::convertible_to<U, T>) Optional(U &&value)
            : value_(std::forward<U>(value)), has_value_{true}
        {
        }

        constexpr ~Optional()
            requires(!std::is_trivially_destructible_v<T>)
        {
            reset();
        }

        constexpr ~Optional()
            requires std::is_trivially_destructible_v<T>
        = default;

        constexpr Optional &operator=(std::nullopt_t) noexcept
        {
            reset();
            return *this;
        }

        constexpr Optional &operator=(const Optional &other)
            requires(std::is_copy_constructible_v<T> && std::is_copy_assignable_v<T> &&
                     !(std::is_trivially_copy_constructible_v<T> && std::is_trivially_copy_assignable_v<T> &&
                       std::is_trivially_destructible_v<T>))
        {
            if (has_value_)
            {
                if (other.has_value_)
                {
                    value_ = other.value_;
                }
                else
                {
                    reset();
                }
            }
            else
            {
                if (other.has_value_)
                {
                    std::construct_at(&value_, other.value_);
                    has_value_ = true;
                }
            }

            return *this;
        }

        constexpr Optional &operator=(const Optional &other)
            requires(std::is_trivially_copy_constructible_v<T> && std::is_trivially_copy_assignable_v<T> &&
                     std::is_trivially_destructible_v<T>)
        = default;

        constexpr Optional &operator=(const Optional &other)
            requires(!std::is_copy_constructible_v<T> || !std::is_copy_assignable_v<T>)
        = delete;

        constexpr Optional &operator=(Optional &&other) noexcept(std::is_nothrow_move_assignable_v<T>)
            requires(std::is_move_constructible_v<T> && std::is_move_assignable_v<T> &&
                     !(std::is_trivially_move_constructible_v<T> && std::is_trivially_move_assignable_v<T> &&
                       std::is_trivially_destructible_v<T>))
        {
            if (has_value_)
            {
                if (other.has_value_)
                {
                    value_ = std::move(other.value_);
                }
                else
                {
                    reset();
                }
            }
            else
            {
                if (other.has_value_)
                {
                    std::construct_at(&value_, std::move(other.value_));
                    has_value_ = true;
                }
            }

            return *this;
        }

        constexpr Optional &operator=(Optional &&other)
            requires(std::is_trivially_move_constructible_v<T> && std::is_trivially_move_assignable_v<T> &&
                     std::is_trivially_destructible_v<T>)
        = default;

        constexpr Optional &operator=(Optional &&other)
            requires(!std::is_move_constructible_v<T> || !std::is_move_assignable_v<T>)
        = delete;

        template <typename U>
            requires std::is_constructible_v<T, const U &> && std::is_assignable_v<T, const U &> &&
                     NotAssignableFromOptional<T, U>
        constexpr Optional &operator=(const Optional<U> &other)
        {
            if (has_value_)
            {
                if (other.has_value())
                {
                    value_ = *other;
                }
                else
                {
                    reset();
                }
            }
            else
            {
                if (other.has_value())
                {
                    std::construct_at(&value_, *other);
                    has_value_ = true;
                }
            }

            return *this;
        }

        template <typename U>
            requires std::is_constructible_v<T, U> && std::is_assignable_v<T, U> && NotAssignableFromOptional<T, U>
        constexpr Optional &operator=(Optional<U> &&other)
        {
            if (has_value_)
            {
                if (other.has_value())
                {
                    value_ = *std::move(other);
                }
                else
                {
                    reset();
                }
            }
            else
            {
                if (other.has_value())
                {
                    std::construct_at(std::addressof(value_), *std::move(other));
                    has_value_ = true;
                }
            }

            return *this;
        }

        template <typename U = std::remove_cv_t<T>>
            requires !std::is_same_v<Optional, std::remove_cvref_t<U>> && std::is_constructible_v<T &, U> &&
                     std::is_assignable_v<T &, U> &&
                     !(std::same_as<std::decay_t<U>, T> || std::is_scalar_v<T>)
                         constexpr Optional &
                     operator=(U &&value)
        {
            if (has_value_)
            {
                value_ = std::forward<U>(value);
            }
            else
            {
                std::construct_at(&value_, std::forward<U>(value));
                has_value_ = true;
            }

            return *this;
        }

        template <typename U, typename... Args>
            requires std::constructible_from<T, std::initializer_list<U> &, Args...>
        constexpr T &emplace(std::initializer_list<U> il, Args &&...args)
        {
            if (has_value_)
            {
                value_ = T{il, std::forward<Args>(args)...};
            }
            else
            {
                std::construct_at(&value_, il, std::forward<Args>(args)...);
                has_value_ = true;
            }

            return value_;
        }

        constexpr void swap(Optional &other) noexcept(std::is_nothrow_move_constructible_v<T> &&
                                                      std::is_nothrow_swappable_v<T>)
        {
            if (has_value_)
            {
                if (other.has_value_)
                {
                    std::swap(value_, other.value_);
                }
                else
                {
                    std::construct_at(&other.value_, std::move(value_));
                    other.has_value_ = true;
                    reset();
                }
            }
            else
            {
                if (other.has_value_)
                {
                    std::construct_at(&value_, std::move(other.value_));
                    has_value_ = true;
                    other.reset();
                }
            }
        }

        constexpr iterator begin() noexcept
        {
            return iterator{has_value_ ? std::addressof(value_) : nullptr};
        }

        constexpr const_iterator begin() const noexcept
        {
            return const_iterator{has_value_ ? std::addressof(value_) : nullptr};
        }

        constexpr iterator end() noexcept
        {
            return std::next(begin(), has_value_ ? 1 : 0);
        }

        constexpr const_iterator end() const noexcept
        {
            return std::next(begin(), has_value_ ? 1 : 0);
        }

        template <typename Self>
        constexpr auto operator->(this Self &self)
        {
            return std::addressof(self.value_);
        }

        template <typename Self>
        constexpr decltype(auto) operator*(this Self &&self)
        {
            if constexpr (!std::is_lvalue_reference_v<Self> && std::is_const_v<std::remove_reference_t<Self>>)
            {
                return static_cast<const T &>(self.value_);
            }
            else
            {
                return std::forward_like<Self>(self.value_);
            }
        }

        constexpr explicit operator bool() const noexcept
        {
            return has_value_;
        }

        [[nodiscard]] constexpr bool has_value() const noexcept
        {
            return has_value_;
        }

        template <typename Self>
        constexpr decltype(auto) value(this Self &&self)
        {

            if !consteval
            {
                if (!self.has_value_)
                {
                    throw std::bad_optional_access{};
                }
            }

            if constexpr (!std::is_lvalue_reference_v<Self> && std::is_const_v<std::remove_reference_t<Self>>)
            {
                return static_cast<const T &>(self.value_);
            }
            else
            {
                return std::forward_like<Self>(self.value_);
            }
        }

        template <typename U = std::remove_cv_t<T>>
        constexpr std::remove_cv_t<T> value_or(U &&u) const &
        {
            if (has_value_)
            {
                return value_;
            }

            return std::forward<U>(u);
        }

        template <typename U = std::remove_cv_t<T>>
        constexpr std::remove_cv_t<T> value_or(U &&u) &&
        {
            if (has_value_)
            {
                return std::move(value_);
            }

            return std::forward<U>(u);
        }

        template <typename Self, typename Functor>
            requires std::invocable<Functor, ForwardLikeType<Self, T>> &&
                     OptionalSpecialization<std::invoke_result_t<Functor, ForwardLikeType<Self, T>>>
        constexpr auto and_then(this Self &&self, Functor &&functor)
        {
            if (self.has_value_)
            {
                return std::invoke(std::forward<Functor>(functor), std::forward<Self>(self).value_);
            }

            return std::invoke_result_t<Functor, ForwardLikeType<Self, T>>{};
        }

        template <typename Self, typename Functor>
            requires std::invocable<Functor, ForwardLikeType<Self, T>> &&
                     (!std::is_void_v<std::invoke_result_t<Functor, ForwardLikeType<Self, T>>>)
        constexpr auto transform(this Self &&self, Functor &&functor)
        {
            if (self.has_value_)
            {
                return Optional<std::invoke_result_t<Functor, ForwardLikeType<Self, T>>>{
                    FromFunctionTag{},
                    std::forward<Functor>(functor),
                    std::forward<Self>(self).value_};
            }

            return Optional<std::invoke_result_t<Functor, ForwardLikeType<Self, T>>>{};
        }

        template <typename Functor>
            requires std::invocable<Functor> && std::copy_constructible<T> &&
                     std::convertible_to<std::invoke_result_t<Functor>, Optional>
        constexpr Optional or_else(Functor &&functor) const &
        {
            return has_value_ ? *this : std::invoke(std::forward<Functor>(functor));
        }

        template <typename Functor>
            requires std::invocable<Functor> && std::move_constructible<T> &&
                     std::convertible_to<std::invoke_result_t<Functor>, Optional>
        constexpr Optional or_else(Functor &&functor) &&
        {
            return has_value_ ? std::move(*this) : std::invoke(std::forward<Functor>(functor));
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr T &emplace(Args &&...args)
        {
            if (has_value_)
            {
                value_ = T(std::forward<Args>(args)...);
            }
            else
            {
                std::construct_at(&value_, std::forward<Args>(args)...);
                has_value_ = true;
            }

            return value_;
        }

        constexpr void reset() noexcept
        {
            if (has_value_)
            {
                std::destroy_at(&value_);
            }

            has_value_ = false;
        }

      private:
        template <typename U>
        friend class Optional;

        template <typename Functor, typename Value>
        constexpr Optional(FromFunctionTag, Functor &&functor, Value &&value)
            : value_{std::invoke(std::forward<Functor>(functor), std::forward<Value>(value))}, has_value_{true}
        {
        }

        union
        {
            std::monostate empty_{};
            T value_;
        };

        bool has_value_{false};
    };

    export template <typename T>
    Optional(T) -> Optional<T>;

    export template <typename T>
    class Optional<T &>
    {
      public:
        using value_type = T;

        using iterator = OptionalIterator<T, Optional>;

        constexpr Optional() noexcept = default;

        constexpr explicit(false) Optional(std::nullopt_t) noexcept
        {
        }

        constexpr explicit(false) Optional(T *value) noexcept
            requires(!std::is_function_v<T>)
            : value_{value}
        {
        }

        constexpr explicit(false) Optional(T &value) noexcept : value_{std::addressof(value)}
        {
        }

        template <typename Arg>
            requires(std::is_constructible_v<T &, Arg> && !ReferenceConvertsFromTemporary<T &, Arg>)
        constexpr explicit Optional(std::in_place_t, Arg &&arg) : value_{get_ref_from_value(arg)}
        {
        }

        template <typename U>
            requires(std::is_constructible_v<T &, U> && !std::is_same_v<std::remove_cvref_t<U>, std::in_place_t> &&
                     !std::is_same_v<std::remove_cvref_t<U>, Optional> && !ReferenceConvertsFromTemporary<T &, U>)
        constexpr explicit(!std::is_convertible_v<U, T &>)
            Optional(U &&value) noexcept(std::is_nothrow_constructible_v<T &, U>)
            : value_{get_ref_from_value(value)}
        {
        }

        template <typename U>
            requires(std::is_constructible_v<T &, U> && !std::is_same_v<std::remove_cvref_t<U>, std::in_place_t> &&
                     !std::is_same_v<std::remove_cvref_t<U>, Optional> && ReferenceConvertsFromTemporary<T &, U>)
        constexpr explicit(!std::is_convertible_v<U, T &>)
            Optional(U &&value) noexcept(std::is_nothrow_constructible_v<T &, U>) = delete;

        template <class U>
            requires(std::is_constructible_v<T &, U &> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && !ReferenceConvertsFromTemporary<T &, U &>)
        constexpr explicit(!std::is_convertible_v<U &, T &>)
            Optional(Optional<U> &rhs) noexcept(std::is_nothrow_constructible_v<T &, U &>)
            : value_{rhs.has_value() ? get_ref_from_value(*rhs) : nullptr}
        {
        }

        template <class U>
            requires(std::is_constructible_v<T &, const U &> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && !ReferenceConvertsFromTemporary<T &, const U &>)
        constexpr explicit(!std::is_convertible_v<const U &, T &>)
            Optional(const Optional<U> &rhs) noexcept(std::is_nothrow_constructible_v<T &, const U &>)
            : value_{rhs.has_value() ? get_ref_from_value(*rhs) : nullptr}
        {
        }

        template <class U>
            requires(std::is_constructible_v<T &, U> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && !ReferenceConvertsFromTemporary<T &, U>)
        constexpr explicit(!std::is_convertible_v<U, T &>)
            Optional(Optional<U> &&rhs) noexcept(std::is_nothrow_constructible_v<T &, U>)
            : value_{rhs.has_value() ? get_ref_from_value(*std::move(rhs)) : nullptr}
        {
        }

        template <class U>
            requires(std::is_constructible_v<T &, const U> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && !ReferenceConvertsFromTemporary<T &, const U>)
        constexpr explicit(!std::is_convertible_v<const U, T &>)
            Optional(const Optional<U> &&rhs) noexcept(std::is_nothrow_constructible_v<T &, const U>)
            : value_{rhs.has_value() ? get_ref_from_value(*std::move(rhs)) : nullptr}
        {
        }

        template <class U>
            requires(std::is_constructible_v<T &, U &> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && ReferenceConvertsFromTemporary<T &, U &>)
        constexpr explicit(!std::is_convertible_v<U &, T &>)
            Optional(Optional<U> &rhs) noexcept(std::is_nothrow_constructible_v<T &, U &>) = delete;

        template <class U>
            requires(std::is_constructible_v<T &, const U &> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && ReferenceConvertsFromTemporary<T &, const U &>)
        constexpr explicit(!std::is_convertible_v<U &, T &>)
            Optional(const Optional<U> &rhs) noexcept(std::is_nothrow_constructible_v<T &, const U &>) = delete;

        template <class U>
            requires(std::is_constructible_v<T &, U> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && ReferenceConvertsFromTemporary<T &, U>)
        constexpr explicit(!std::is_convertible_v<U &, T &>)
            Optional(Optional<U> &&rhs) noexcept(std::is_nothrow_constructible_v<T &, U>) = delete;

        template <class U>
            requires(std::is_constructible_v<T &, const U> && !std::is_same_v<std::remove_cv_t<T>, Optional<U>> &&
                     !std::is_same_v<T &, U> && ReferenceConvertsFromTemporary<T &, const U>)
        constexpr explicit(!std::is_convertible_v<U &, T &>)
            Optional(const Optional<U> &&rhs) noexcept(std::is_nothrow_constructible_v<T &, const U>) = delete;

        constexpr Optional &operator=(std::nullopt_t) noexcept
        {
            reset();
            return *this;
        }

        template <typename U>
            requires std::is_constructible_v<T &, U> && !ReferenceConvertsFromTemporary<T &, U>
                                                            constexpr T &
                     emplace(U && value) noexcept(std::is_nothrow_constructible_v<T &, U>)
        {
            value_ = get_ref_from_value(value);
            return *value_;
        }

        constexpr void swap(Optional &other) noexcept
        {
            std::swap(value_, other.value_);
        }

        constexpr iterator begin() const noexcept
        {
            return iterator{value_};
        }

        constexpr iterator end() const noexcept
        {
            return value_ != nullptr ? iterator{std::next(value_)} : iterator{};
        }

        constexpr T *operator->() const noexcept
        {
            return value_;
        }

        constexpr T &operator*() const noexcept
        {
            return *value_;
        }

        constexpr explicit operator bool() const noexcept
        {
            return value_ != nullptr;
        }

        [[nodiscard]] constexpr bool has_value() const noexcept
        {
            return value_ != nullptr;
        }

        constexpr T &value() const
        {
            if (value_ == nullptr)
            {
                throw std::bad_optional_access{};
            }

            return *value_;
        }

        template <typename U = std::remove_cv_t<T>>
            requires(std::is_object_v<T> && !std::is_array_v<T>)
        constexpr std::decay_t<T> value_or(U &&value) const
        {
            if (value_ != nullptr)
            {
                return *value_;
            }

            return std::decay_t<T>{std::forward<U>(value)};
        }

        template <std::invocable<T &> Functor>
            requires OptionalSpecialization<std::invoke_result_t<Functor, T &>>
        constexpr auto and_then(Functor &&functor) const
        {
            if (value_ != nullptr)
            {
                return std::invoke(std::forward<Functor>(functor), *value_);
            }

            return std::invoke_result_t<Functor, T &>{};
        }

        template <std::invocable<T &> Functor>
        constexpr Optional<std::invoke_result_t<Functor, T &>> transform(Functor &&functor) const
        {
            if (value_ != nullptr)
            {
                return Optional<std::invoke_result_t<Functor, T &>>{FromFunctionTag{},
                                                                    std::forward<Functor>(functor),
                                                                    *value_};
            }

            return std::nullopt;
        }

        template <std::invocable Functor>
            requires std::convertible_to<std::invoke_result_t<Functor>, Optional>
        constexpr Optional or_else(Functor &&functor) const
        {
            if (value_ != nullptr)
            {
                return *this;
            }

            return std::invoke(std::forward<Functor>(functor));
        }

        constexpr void reset() noexcept
        {
            value_ = nullptr;
        }

      private:
        template <typename U>
        friend class Optional;

        template <typename Functor, typename Value>
        constexpr Optional(FromFunctionTag, Functor &&functor, Value &&value)
            : value_{get_ref_from_value(std::invoke(std::forward<Functor>(functor), std::forward<Value>(value)))}
        {
        }

        template <typename U>
        static T *get_ref_from_value(U &&other)
        {
            T &ref(std::forward<U>(other));
            return std::addressof(ref);
        }

        T *value_{nullptr};
    };

    export template <typename T>
    constexpr Optional<std::decay_t<T>> make_optional(T &&value)
    {
        return Optional<std::decay_t<T>>{std::forward<T>(value)};
    }

    export template <typename T, typename... Args>
        requires std::constructible_from<T, Args...>
    constexpr Optional<T> make_optional(Args &&...args)
    {
        return Optional<T>{std::in_place, std::forward<Args>(args)...};
    }

    export template <typename T, typename U, typename... Args>
        requires std::constructible_from<T, std::initializer_list<U> &, Args...>
    constexpr Optional<T> make_optional(std::initializer_list<U> il, Args &&...args) noexcept(
        std::is_nothrow_constructible_v<T, std::initializer_list<U> &, Args...>)
    {
        return Optional<T>{std::in_place, il, std::forward<Args>(args)...};
    }

    export template <typename T, typename U>
        requires EqualityComparableWith<T, U>
    constexpr bool operator==(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return lhs.has_value() == rhs.has_value() && (!lhs.has_value() || *lhs == *rhs);
    }

    export template <typename T, typename U>
        requires InequalityComparableWith<T, U>
    constexpr bool operator!=(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return lhs.has_value() != rhs.has_value() || (lhs.has_value() && *lhs != *rhs);
    }

    export template <typename T, typename U>
        requires LessThanComparableWith<T, U>
    constexpr bool operator<(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return rhs.has_value() && (!lhs.has_value() || *lhs < *rhs);
    }

    export template <typename T, typename U>
        requires LessThanOrEqualComparableWith<T, U>
    constexpr bool operator<=(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return !lhs.has_value() || (rhs.has_value() && *lhs <= *rhs);
    }

    export template <typename T, typename U>
        requires GreaterThanComparableWith<T, U>
    constexpr bool operator>(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return lhs.has_value() && (!rhs.has_value() || *lhs > *rhs);
    }

    export template <typename T, typename U>
        requires GreaterThanOrEqualComparableWith<T, U>
    constexpr bool operator>=(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return rhs.has_value() && (!lhs.has_value() || *lhs >= *rhs);
    }

    export template <typename T, std::three_way_comparable_with<T> U>
    constexpr std::compare_three_way_result_t<T, U> operator<=>(const Optional<T> &lhs, const Optional<U> &rhs)
    {
        return lhs.has_value() && rhs.has_value() ? *lhs <=> *rhs : lhs.has_value() <=> rhs.has_value();
    }

    export template <typename T>
    constexpr bool operator==(const Optional<T> &lhs, std::nullopt_t) noexcept
    {
        return !lhs.has_value();
    }

    export template <typename T>
    constexpr std::strong_ordering operator<=>(const Optional<T> &lhs, std::nullopt_t) noexcept
    {
        return lhs.has_value() <=> false;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && EqualityComparableWith<T, U>
    constexpr bool operator==(const Optional<T> &lhs, const U &rhs)
    {
        return lhs.has_value() && *lhs == rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && EqualityComparableWith<U, T>
    constexpr bool operator==(const U &lhs, const Optional<T> &rhs)
    {
        return rhs.has_value() && lhs == *rhs;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && InequalityComparableWith<T, U>
    constexpr bool operator!=(const Optional<T> &lhs, const U &rhs)
    {
        return !lhs.has_value() || *lhs != rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && InequalityComparableWith<U, T>
    constexpr bool operator!=(const U &lhs, const Optional<T> &rhs)
    {
        return !rhs.has_value() || lhs != *rhs;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && LessThanComparableWith<T, U>
    constexpr bool operator<(const Optional<T> &lhs, const U &rhs)
    {
        return !lhs.has_value() || *lhs < rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && LessThanComparableWith<U, T>
    constexpr bool operator<(const U &lhs, const Optional<T> &rhs)
    {
        return rhs.has_value() && lhs < *rhs;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && LessThanOrEqualComparableWith<T, U>
    constexpr bool operator<=(const Optional<T> &lhs, const U &rhs)
    {
        return !lhs.has_value() || *lhs <= rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && LessThanOrEqualComparableWith<U, T>
    constexpr bool operator<=(const U &lhs, const Optional<T> &rhs)
    {
        return rhs.has_value() && lhs <= *rhs;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && GreaterThanComparableWith<T, U>
    constexpr bool operator>(const Optional<T> &lhs, const U &rhs)
    {
        return lhs.has_value() && *lhs > rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && GreaterThanComparableWith<U, T>
    constexpr bool operator>(const U &lhs, const Optional<T> &rhs)
    {
        return !rhs.has_value() || lhs > *rhs;
    }

    export template <typename T, typename U>
        requires !OptionalSpecialization<U> && GreaterThanOrEqualComparableWith<T, U>
    constexpr bool operator>=(const Optional<T> &lhs, const U &rhs)
    {
        return lhs.has_value() && *lhs >= rhs;
    }

    export template <typename U, typename T>
        requires !OptionalSpecialization<U> && GreaterThanOrEqualComparableWith<U, T>
    constexpr bool operator>=(const U &lhs, const Optional<T> &rhs)
    {
        return !rhs.has_value() || lhs >= *rhs;
    }

    export template <typename T, std::three_way_comparable_with<T> U>
        requires !OptionalSpecialization<U>
                 constexpr std::compare_three_way_result_t<T, U> operator<=>(const Optional<T> &lhs, const U &rhs)
    {
        return lhs.has_value() ? *lhs <=> rhs : std::strong_ordering::less;
    }

    export template <typename T>
    constexpr void swap(Optional<T> &lhs, Optional<T> &rhs) noexcept(noexcept(lhs.swap(rhs)))
    {
        lhs.swap(rhs);
    }
} // namespace retro

export template <typename T>
constexpr bool std::ranges::enable_view<retro::Optional<T>> = true;

export template <typename T>
constexpr bool std::ranges::enable_borrowed_range<retro::Optional<T &>> = true;

export template <typename T>
constexpr auto std::format_kind<retro::Optional<T>> = std::range_format::disabled;

export template <retro::Hashable T>
    requires(!std::is_reference_v<T>)
struct std::hash<retro::Optional<T>>
{
    usize operator()(const retro::Optional<T> &optional) const
        noexcept(noexcept(hash<std::remove_const_t<T>>{}(*optional)))
    {
        if (optional.has_value())
        {
            return std::hash<std::remove_const_t<T>>{}(*optional);
        }

        return 0;
    }
};
