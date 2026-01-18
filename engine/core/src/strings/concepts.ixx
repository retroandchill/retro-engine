/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:strings.concepts;

import std;

import uni_algo;

namespace retro
{
    /**
     * Concept to ensure that the underlying type is a character.
     */
    export template <typename T>
    concept Char = std::same_as<T, char> || std::same_as<T, wchar_t> || std::same_as<T, char8_t> ||
                   std::same_as<T, char16_t> || std::same_as<T, char32_t>;

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

    template <>
    struct UnaConverter<Encoding::Utf8, Encoding::Utf16>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf8 && ENCODING_OF<To> == Encoding::Utf16)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf8to16<From, To>(source, std::move(allocator));
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf16, Encoding::Utf8>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf16 && ENCODING_OF<To> == Encoding::Utf8)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf16to8<From, To>(source, std::move(allocator));
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf8, Encoding::Utf32>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf8 && ENCODING_OF<To> == Encoding::Utf32)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf8to32<From, To>(source, std::move(allocator));
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf32, Encoding::Utf8>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf32 && ENCODING_OF<To> == Encoding::Utf8)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf32to8<From, To>(source, std::move(allocator));
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf16, Encoding::Utf32>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf16 && ENCODING_OF<To> == Encoding::Utf32)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf16to32<From, To>(source, std::move(allocator));
        }
    };

    template <>
    struct UnaConverter<Encoding::Utf32, Encoding::Utf16>
    {
        template <Char To, Char From, typename Allocator>
            requires(ENCODING_OF<From> == Encoding::Utf32 && ENCODING_OF<To> == Encoding::Utf16)
        static constexpr auto convert(std::basic_string_view<From> source, Allocator allocator)
        {
            return una::utf32to16<From, To>(source, std::move(allocator));
        }
    };

    export template <Char To, Char From, typename Allocator = std::allocator<To>>
    constexpr auto convert_string(std::basic_string_view<From> source, Allocator allocator = Allocator{})
    {
        if constexpr (std::same_as<To, From>)
        {
            return std::basic_string<To, std::char_traits<To>, Allocator>{source, std::move(allocator)};
        }
        else
        {
            return UnaConverter<ENCODING_OF<From>, ENCODING_OF<To>>::template convert<To>(source, std::move(allocator));
        }
    }

    export template <Char To,
                     Char From,
                     typename FromTraits,
                     typename FromAllocator,
                     typename ToAllocator = std::allocator<To>>
    constexpr auto convert_string(const std::basic_string<From, FromTraits, FromAllocator> &source,
                                  ToAllocator allocator = ToAllocator{})
    {
        return convert_string<To>(std::basic_string_view<From>{source}, std::move(allocator));
    }
} // namespace retro
