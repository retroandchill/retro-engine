/**
 * @file encoding.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.strings.encoding;

import std;
import retro.core.type_traits.basic;
import :uni_algo;

namespace retro
{
    export enum class StringComparison : std::uint8_t
    {
        CaseSensitive,
        CaseInsensitive
    };

    enum class Encoding
    {
        Utf8,
        Utf16,
        Utf32
    };

    template <Char C>
    struct EncodingOf;

    template <>
    struct EncodingOf<char>
    {
        static constexpr auto value = Encoding::Utf8;
    };

    template <>
    struct EncodingOf<char8_t>
    {
        static constexpr auto value = Encoding::Utf8;
    };

    template <>
    struct EncodingOf<char16_t>
    {
        static constexpr auto value = Encoding::Utf16;
    };

    template <>
    struct EncodingOf<char32_t>
    {
        static constexpr auto value = Encoding::Utf32;
    };

    template <>
    struct EncodingOf<wchar_t>
    {
        static constexpr auto value = sizeof(wchar_t) == 2 ? Encoding::Utf16 : Encoding::Utf32;
    };

    template <Char C>
    constexpr Encoding ENCODING_OF = EncodingOf<C>::value;

    template <Encoding FromEnc, Encoding ToEnc>
    struct UnaConverter;

    template <Encoding Encoding>
    struct UnaConverter<Encoding, Encoding>
    {
        template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding && ENCODING_OF<To> == Encoding)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<To, From>)
            {
                return std::basic_string<To, std::char_traits<To>, Allocator>{std::from_range,
                                                                              std::forward<Range>(source),
                                                                              std::move(allocator)};
            }
            else
            {
                static_assert(sizeof(To) == sizeof(From), "Cannot convert between different sized encodings.");

                // Since we've already confirmed we're looking at the same size, we can simply use std::bit_cast
                // to force a conversion
                return std::basic_string<To, std::char_traits<To>, Allocator>{
                    std::from_range,
                    std::forward<Range>(source) | std::views::transform([](From c) { return std::bit_cast<To>(c); }),
                    std::move(allocator)};
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf8, Encoding::Utf16>
    {
        template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf8 &&
                     ENCODING_OF<To> == Encoding::Utf16)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf8to16<From, To>(source, std::move(allocator));
            }
            else if constexpr (std::ranges::contiguous_range<Range> && std::ranges::sized_range<Range>)
            {
                return una::utf8to16<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
            else
            {
                return una::ranges::to_utf16<std::basic_string<To, std::char_traits<To>, Allocator>>(
                    una::views::utf8(std::forward<Range>(source)),
                    std::move(allocator));
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf16, Encoding::Utf8>
    {
        template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf16 &&
                     ENCODING_OF<To> == Encoding::Utf8)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf16to8<From, To>(source, std::move(allocator));
            }
            else if constexpr (std::ranges::contiguous_range<Range> && std::ranges::sized_range<Range>)
            {
                return una::utf16to8<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
            else
            {
                return una::ranges::to_utf8<std::basic_string<To, std::char_traits<To>, Allocator>>(
                    una::views::utf16(std::forward<Range>(source)),
                    std::move(allocator));
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf8, Encoding::Utf32>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf8 &&
                     ENCODING_OF<To> == Encoding::Utf32 && std::ranges::sized_range<Range>)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf8to32<From, To>(source, std::move(allocator));
            }
            else
            {
                return una::utf8to32<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf32, Encoding::Utf8>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf32 &&
                     ENCODING_OF<To> == Encoding::Utf8 && std::ranges::sized_range<Range>)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf32to8<From, To>(source, std::move(allocator));
            }
            else
            {
                return una::utf32to8<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf16, Encoding::Utf32>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf16 &&
                     ENCODING_OF<To> == Encoding::Utf32 && std::ranges::sized_range<Range>)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf16to32<From, To>(source, std::move(allocator));
            }
            else
            {
                return una::utf16to32<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf32, Encoding::Utf16>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::Utf32 &&
                     ENCODING_OF<To> == Encoding::Utf16 && std::ranges::sized_range<Range>)
        static constexpr auto convert(Range &&source, Allocator allocator)
        {
            using From = std::ranges::range_value_t<Range>;
            if constexpr (std::same_as<std::decay_t<Range>, std::basic_string_view<From>>)
            {
                return una::utf32to16<From, To>(source, std::move(allocator));
            }
            else
            {
                return una::utf32to16<From, To>(
                    std::basic_string_view<From>{std::ranges::data(source), std::ranges::size(source)},
                    std::move(allocator));
            }
        }
    };

    export template <typename Range, typename To>
    concept EncodableRange =
        Char<To> && std::ranges::input_range<Range> && Char<std::ranges::range_value_t<Range>> &&
        ((ENCODING_OF<To> != Encoding::Utf32 && ENCODING_OF<std::ranges::range_value_t<Range>> != Encoding::Utf32) ||
         (std::ranges::contiguous_range<Range> && std::ranges::sized_range<Range>));

    export template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator = std::allocator<To>>
        requires std::same_as<To, typename Allocator::value_type> && Char<std::ranges::range_value_t<Range>> &&
                 EncodableRange<Range, To>
    constexpr auto convert_string(Range &&source, Allocator allocator = Allocator{})
    {
        using From = std::ranges::range_value_t<Range>;
        return UnaConverter<ENCODING_OF<From>, ENCODING_OF<To>>::template convert<To>(std::forward<Range>(source),
                                                                                      std::move(allocator));
    }

    export template <Char CharType, SimpleAllocator Allocator = std::allocator<CharType>>
        requires(ENCODING_OF<CharType> == Encoding::Utf8 || ENCODING_OF<CharType> == Encoding::Utf16)
    constexpr auto to_lower(std::basic_string_view<CharType> view, Allocator allocator = Allocator{})
    {
        if constexpr (ENCODING_OF<CharType> == Encoding::Utf8)
        {
            return una::cases::to_lowercase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::Utf16);
            return una::cases::to_lowercase_utf16(view, std::move(allocator));
        }
    }

    export template <Char CharType, SimpleAllocator Allocator = std::allocator<CharType>>
        requires(ENCODING_OF<CharType> == Encoding::Utf8 || ENCODING_OF<CharType> == Encoding::Utf16)
    constexpr auto to_upper(std::basic_string_view<CharType> view, Allocator allocator = Allocator{})
    {
        if constexpr (ENCODING_OF<CharType> == Encoding::Utf8)
        {
            return una::cases::to_uppercase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::Utf16);
            return una::cases::to_uppercase_utf16(view, std::move(allocator));
        }
    }

    export template <Char CharType, SimpleAllocator Allocator = std::allocator<CharType>>
        requires(ENCODING_OF<CharType> == Encoding::Utf8 || ENCODING_OF<CharType> == Encoding::Utf16)
    constexpr auto to_titlecase(std::basic_string_view<CharType> view, Allocator allocator = Allocator{})
    {
        if constexpr (ENCODING_OF<CharType> == Encoding::Utf8)
        {
            return una::cases::to_titlecase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::Utf16);
            return una::cases::to_titlecase_utf16(view, std::move(allocator));
        }
    }

    export template <StringComparison Comparison = StringComparison::CaseSensitive, Char CharType>
        requires(ENCODING_OF<CharType> == Encoding::Utf8 || ENCODING_OF<CharType> == Encoding::Utf16)
    constexpr std::strong_ordering compare(std::basic_string_view<CharType> lhs, std::basic_string_view<CharType> rhs)
    {
        if constexpr (Comparison == StringComparison::CaseSensitive)
        {
            if constexpr (ENCODING_OF<CharType> == Encoding::Utf8)
            {
                return std::strong_ordering{static_cast<std::int8_t>(una::casesens::compare_utf8(lhs, rhs))};
            }
            else
            {
                static_asset(ENCODING_OF<CharType> == Encoding::Utf16);
                return std::strong_ordering{static_cast<std::int8_t>(una::casesens::compare_utf16(lhs, rhs))};
            }
        }
        else
        {
            static_assert(Comparison == StringComparison::CaseInsensitive);

            if constexpr (ENCODING_OF<CharType> == Encoding::Utf8)
            {
                return std::strong_ordering{static_cast<std::int8_t>(una::caseless::compare_utf8(lhs, rhs))};
            }
            else
            {
                static_asset(ENCODING_OF<CharType> == Encoding::Utf16);
                return std::strong_ordering{static_cast<std::int8_t>(una::caseless::compare_utf16(lhs, rhs))};
            }
        }
    }
} // namespace retro
