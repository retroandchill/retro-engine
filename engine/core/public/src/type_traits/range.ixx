/**
 * @file range.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.range;

import std;
import retro.core.type_traits.basic;

namespace retro
{
    export template <std::integral T>
    constexpr T index_none = static_cast<T>(-1);

    export template <typename Container, typename Reference>
    concept ContainerAppendable = requires(Container &c, Reference &&ref) {
        requires requires { c.emplace_back(std::forward<Reference>(ref)); } ||
                     requires { c.push_back(std::forward<Reference>(ref)); } ||
                     requires { c.emplace(c.end(), std::forward<Reference>(ref)); } ||
                     requires { c.insert(c.end(), std::forward<Reference>(ref)); };
    };

    export template <typename R, typename T>
    concept ContainerCompatibleRange =
        std::ranges::input_range<R> && std::convertible_to<std::ranges::range_reference_t<R>, T>;

    export template <typename T>
    concept CharTraits =
        requires {
            typename T::char_type;
            typename T::int_type;
            typename T::off_type;
            typename T::pos_type;
            typename T::state_type;
        } && requires(typename T::char_type c1,
                      typename T::char_type c2,
                      typename T::char_type *s,
                      const typename T::char_type *cs,
                      std::size_t n) {
            {
                T::eq(c1, c2)
            } -> std::convertible_to<bool>;
            {
                T::lt(c1, c2)
            } -> std::convertible_to<bool>;
            {
                T::compare(cs, cs, n)
            } -> std::convertible_to<int>;
            {
                T::length(cs)
            } -> std::convertible_to<std::size_t>;
            {
                T::find(cs, n, c1)
            } -> std::same_as<const T::char_type *>;
            {
                T::move(s, cs, n)
            } -> std::same_as<typename T::char_type *>;
            {
                T::copy(s, cs, n)
            } -> std::same_as<typename T::char_type *>;
            {
                T::assign(s, n, c1)
            } -> std::same_as<typename T::char_type *>;
            {
                T::not_eof(T::to_int_type(c1))
            } -> std::same_as<typename T::int_type>;

            // Additional requirements (noexcept)
            requires noexcept(T::eq(c1, c2));
        };

    template <typename T>
    struct StringLikeTraits;

    template <typename T>
    concept ValidStringInfo = requires {
        typename T::value_type;
        requires Char<typename T::value_type>;
        typename T::traits_type;
        requires CharTraits<typename T::traits_type>;
        requires std::same_as<typename T::value_type, typename T::traits_type::char_type>;
    };

    template <typename T>
        requires ValidStringInfo<std::remove_cvref_t<T>>
    struct StringLikeTraits<T>
    {
        using value_type = std::remove_cvref_t<T>::value_type;
        using traits_type = std::remove_cvref_t<T>::traits_type;
    };

    template <Char T>
    struct StringLikeTraits<T *>
    {
        using value_type = std::remove_cv_t<T>;
        using traits_type = std::char_traits<value_type>;
    };

    export template <typename T>
        requires ValidStringInfo<StringLikeTraits<T>>
    using StringViewType =
        std::basic_string_view<typename StringLikeTraits<T>::value_type, typename StringLikeTraits<T>::traits_type>;

    export template <typename T>
    concept StringViewConvertible = ValidStringInfo<StringLikeTraits<T>> && std::convertible_to<T, StringViewType<T>>;

    export template <StringViewConvertible T>
    constexpr StringViewType<T> to_string_view(T &&str)
    {
        return std::forward<T>(str);
    }
} // namespace retro
