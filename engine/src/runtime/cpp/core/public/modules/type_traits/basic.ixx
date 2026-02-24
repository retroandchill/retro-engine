/**
 * @file basic.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.basic;

import std;

namespace retro
{
    template <bool IsConst, class T>
    using MaybeConst = std::conditional_t<IsConst, const T, T>;

    template <class T,
              class U,
              class Tmp = MaybeConst<std::is_const_v<std::remove_reference_t<T>>, std::remove_reference_t<U>>>
    using ForwardLikeTypeHelper = std::conditional_t<std::is_rvalue_reference_v<T &&>, Tmp &&, Tmp &>;

    export template <typename T, typename U>
    using ForwardLikeType = ForwardLikeTypeHelper<T, U>;

    export template <typename To, typename From>
    concept ReferenceConvertsFromTemporary =
        std::is_reference_v<To> && ((!std::is_reference_v<From> &&
                                     std::is_convertible_v<std::remove_cvref_t<From> *, std::remove_cvref_t<To> *>) ||
                                    (std::is_lvalue_reference_v<To> && std::is_const_v<std::remove_reference_t<To>> &&
                                     std::is_convertible_v<From, const std::remove_cvref_t<To> &&> &&
                                     !std::is_convertible_v<From, std::remove_cvref_t<To> &>));

    export template <typename From, typename To>
    concept CanStaticCast = requires(From &&from) {
        {
            static_cast<To>(std::forward<From>(from))
        } -> std::same_as<To>;
    };

    /**
     * Concept to ensure that the underlying type is a character.
     */
    export template <typename T>
    concept Char = std::same_as<T, char> || std::same_as<T, wchar_t> || std::same_as<T, char8_t> ||
                   std::same_as<T, char16_t> || std::same_as<T, char32_t>;

    export template <typename T>
    concept Numeric =
        std::same_as<T, std::remove_cvref_t<T>> && std::is_arithmetic_v<T> && !std::same_as<T, bool> && !Char<T>;

    /**
     * Public export of the concept for the minimum requirements for a type to be a valid allocator
     * as published in the C++26 standard.
     *
     * @tparam Alloc The allocator type under test.
     */
    export template <class Alloc>
    concept SimpleAllocator = requires(Alloc alloc, std::size_t n) {
        // ReSharper disable once CppRedundantTypenameKeyword
        {
            *alloc.allocate(n)
        } -> std::same_as<typename Alloc::value_type &>;
        {
            alloc.deallocate(alloc.allocate(n), n)
        };
    } && std::copy_constructible<Alloc> && std::equality_comparable<Alloc>;

    export template <typename T>
    concept DecayCopyable = std::constructible_from<std::decay_t<T>, T>;

    export template <std::size_t N>
    using SmallestSize = std::conditional_t < N < std::numeric_limits<std::uint8_t>::max(),
          std::uint8_t, std::conditional_t < N < std::numeric_limits<std::uint16_t>::max(), std::uint16_t,
          std::conditional_t<
              N<std::numeric_limits<std::uint32_t>::max(),
                std::uint32_t,
                std::conditional_t<N<std::numeric_limits<std::uint64_t>::max(), std::uint64_t, std::size_t>>>>;

    export template <typename T>
    concept FullyTrivial = std::is_trivially_copyable_v<T> && std::is_trivially_default_constructible_v<T> &&
                           std::is_trivially_destructible_v<T>;

    export template <typename T, typename U>
    concept SameUnqualifed = std::same_as<std::remove_cv_t<T>, std::remove_cv_t<U>>;

    template <typename T>
    struct CvQualRank : std::integral_constant<std::size_t, std::is_const_v<T> + std::is_volatile_v<T>>
    {
    };

    export template <typename T>
    constexpr std::size_t cv_qual_rank = CvQualRank<T>::value;

    export template <typename T, typename U>
    concept LessCvQualified = cv_qual_rank<T> < cv_qual_rank<U>;

    export template <typename Base, typename Derived>
    concept ProperBaseOf =
        std::derived_from<std::remove_cv_t<Derived>, std::remove_cv_t<Base>> && !SameUnqualifed<Base, Derived>;
} // namespace retro
