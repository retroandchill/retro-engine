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
        case_sensitive,
        case_insensitive
    };

    enum class Encoding
    {
        utf8,
        utf16,
        utf32
    };

    template <Char C>
    struct EncodingOf;

    template <>
    struct EncodingOf<char>
    {
        static constexpr auto value = Encoding::utf8;
    };

    template <>
    struct EncodingOf<char8_t>
    {
        static constexpr auto value = Encoding::utf8;
    };

    template <>
    struct EncodingOf<char16_t>
    {
        static constexpr auto value = Encoding::utf16;
    };

    template <>
    struct EncodingOf<char32_t>
    {
        static constexpr auto value = Encoding::utf32;
    };

    template <>
    struct EncodingOf<wchar_t>
    {
        static constexpr auto value = sizeof(wchar_t) == 2 ? Encoding::utf16 : Encoding::utf32;
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
    struct UnaConverter<Encoding::utf8, Encoding::utf16>
    {
        template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf8 &&
                     ENCODING_OF<To> == Encoding::utf16)
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
    struct UnaConverter<Encoding::utf16, Encoding::utf8>
    {
        template <Char To, std::ranges::input_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf16 &&
                     ENCODING_OF<To> == Encoding::utf8)
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
    struct UnaConverter<Encoding::utf8, Encoding::utf32>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf8 &&
                     ENCODING_OF<To> == Encoding::utf32 && std::ranges::sized_range<Range>)
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
    struct UnaConverter<Encoding::utf32, Encoding::utf8>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf32 &&
                     ENCODING_OF<To> == Encoding::utf8 && std::ranges::sized_range<Range>)
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
    struct UnaConverter<Encoding::utf16, Encoding::utf32>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf16 &&
                     ENCODING_OF<To> == Encoding::utf32 && std::ranges::sized_range<Range>)
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
    struct UnaConverter<Encoding::utf32, Encoding::utf16>
    {
        template <Char To, std::ranges::contiguous_range Range, SimpleAllocator Allocator>
            requires(ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf32 &&
                     ENCODING_OF<To> == Encoding::utf16 && std::ranges::sized_range<Range>)
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
        ((ENCODING_OF<To> != Encoding::utf32 && ENCODING_OF<std::ranges::range_value_t<Range>> != Encoding::utf32) ||
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

    export template <std::ranges::contiguous_range Range,
                     SimpleAllocator Allocator = std::allocator<std::ranges::range_value_t<Range>>>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>> &&
                 (ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf8 ||
                  ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf16))
    constexpr auto to_lower(Range &&range, Allocator allocator = Allocator{})
    {
        using CharType = std::ranges::range_value_t<Range>;
        std::basic_string_view<CharType> view{std::ranges::data(range), std::ranges::size(range)};
        if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
        {
            return una::cases::to_lowercase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::utf16);
            return una::cases::to_lowercase_utf16(view, std::move(allocator));
        }
    }

    export template <std::ranges::contiguous_range Range,
                     SimpleAllocator Allocator = std::allocator<std::ranges::range_value_t<Range>>>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>> &&
                 (ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf8 ||
                  ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf16))
    constexpr auto to_upper(Range &&range, Allocator allocator = Allocator{})
    {
        using CharType = std::ranges::range_value_t<Range>;
        std::basic_string_view<CharType> view{std::ranges::data(range), std::ranges::size(range)};
        if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
        {
            return una::cases::to_uppercase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::utf16);
            return una::cases::to_uppercase_utf16(view, std::move(allocator));
        }
    }

    export template <std::ranges::contiguous_range Range,
                     SimpleAllocator Allocator = std::allocator<std::ranges::range_value_t<Range>>>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>> &&
                 (ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf8 ||
                  ENCODING_OF<std::ranges::range_value_t<Range>> == Encoding::utf16))
    constexpr auto to_titlecase(Range &&range, Allocator allocator = Allocator{})
    {
        using CharType = std::ranges::range_value_t<Range>;
        std::basic_string_view<CharType> view{std::ranges::data(range), std::ranges::size(range)};
        if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
        {
            return una::cases::to_titlecase_utf8(view, std::move(allocator));
        }
        else
        {
            static_assert(ENCODING_OF<CharType> == Encoding::utf16);
            return una::cases::to_titlecase_utf16(view, std::move(allocator));
        }
    }

    export template <StringComparison Comparison = StringComparison::case_sensitive,
                     std::ranges::contiguous_range RangeA,
                     std::ranges::contiguous_range RangeB>
        requires(std::same_as<std::ranges::range_value_t<RangeA>, std::ranges::range_value_t<RangeB>> &&
                 (ENCODING_OF<std::ranges::range_value_t<RangeA>> == Encoding::utf8 ||
                  ENCODING_OF<std::ranges::range_value_t<RangeA>> == Encoding::utf16))
    constexpr std::strong_ordering compare(RangeA &&lhs, RangeB &&rhs)
    {
        using CharType = std::ranges::range_value_t<RangeA>;
        if constexpr (Comparison == StringComparison::case_sensitive)
        {
            if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
            {
                return std::strong_ordering{static_cast<std::int8_t>(una::casesens::compare_utf8(lhs, rhs))};
            }
            else
            {
                static_assert(ENCODING_OF<CharType> == Encoding::utf16);
                return std::strong_ordering{static_cast<std::int8_t>(una::casesens::compare_utf16(lhs, rhs))};
            }
        }
        else
        {
            static_assert(Comparison == StringComparison::case_insensitive);

            if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
            {
                return std::strong_ordering{static_cast<std::int8_t>(una::caseless::compare_utf8(lhs, rhs))};
            }
            else
            {
                static_assert(ENCODING_OF<CharType> == Encoding::utf16);
                return std::strong_ordering{static_cast<std::int8_t>(una::caseless::compare_utf16(lhs, rhs))};
            }
        }
    }

    struct ToCodepoint : std::ranges::range_adaptor_closure<ToCodepoint>
    {

        template <std::ranges::input_range Range>
            requires Char<std::ranges::range_value_t<Range>>
        constexpr decltype(auto) operator()(Range &&range) const
        {
            using CharType = std::ranges::range_value_t<Range>;
            if constexpr (ENCODING_OF<CharType> == Encoding::utf8)
            {
                return range | una::views::utf8;
            }
            else if constexpr (ENCODING_OF<CharType> == Encoding::utf16)
            {
                return range | una::views::utf16;
            }
            else
            {
                return std::forward<Range>(range);
            }
        }
    };

    export constexpr ToCodepoint to_codepoint{};

    export template <std::ranges::input_range Range>
        requires Char<std::ranges::range_value_t<Range>>
    constexpr bool is_empty_or_whitespace(Range &&view)
    {
        if constexpr (std::ranges::sized_range<Range>)
        {
            if (std::ranges::size(view) == 0)
                return true;
        }

        std::ranges::all_of(view | to_codepoint, [](const char32_t c) { return !una::codepoint::is_whitespace(c); });

        return true;
    }

    export template <std::ranges::contiguous_range Range>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>>)
    constexpr std::basic_string_view<std::ranges::range_value_t<Range>> trim(Range &&range)
    {
        using CharType = std::ranges::range_value_t<Range>;

        auto data = std::ranges::data(range);
        auto size = std::ranges::size(range);

        std::size_t start = 0;
        while (start < size && std::isspace(static_cast<std::int32_t>(data[start])))
        {
            ++start;
        }

        if (start == size)
            return {};

        std::size_t end = size;
        while (end > start && std::isspace(static_cast<std::int32_t>(data[end - 1])))
        {
            --end;
        }

        return std::basic_string_view<CharType>{data + start, end - start};
    }

    export template <std::ranges::contiguous_range Range>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>>)
    constexpr std::basic_string_view<std::ranges::range_value_t<Range>> trim_start(Range &&range)
    {
        using CharType = std::ranges::range_value_t<Range>;

        auto data = std::ranges::data(range);
        auto size = std::ranges::size(range);

        std::size_t start = 0;
        while (start < size && std::isspace(static_cast<std::int32_t>(data[start])))
        {
            ++start;
        }

        if (start == size)
            return {};

        return std::basic_string_view<CharType>{data + start, size - start};
    }

    export template <std::ranges::contiguous_range Range>
        requires(std::ranges::sized_range<Range> && Char<std::ranges::range_value_t<Range>>)
    constexpr std::basic_string_view<std::ranges::range_value_t<Range>> trim_end(Range &&range)
    {
        using CharType = std::ranges::range_value_t<Range>;

        auto data = std::ranges::data(range);
        auto size = std::ranges::size(range);

        constexpr std::size_t start = 0;
        std::size_t end = size;
        while (end > start && std::isspace(static_cast<std::int32_t>(data[end - 1])))
        {
            --end;
        }

        return std::basic_string_view<CharType>{data + start, end - start};
    }
} // namespace retro
