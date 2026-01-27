/**
 * @file strings.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

export module retro.core:strings;

import std;
import :concepts;
export import :uni_algo;
import :defines;
export import :fmt;

namespace retro
{
    export enum class StringComparison : uint8
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
                return std::forward<Range>(source) | una::views::utf8 |
                       una::ranges::to_utf16<std::basic_string<To, std::char_traits<To>, Allocator>>(
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
                return std::forward<Range>(source) | una::views::utf16 |
                       una::ranges::to_utf8<std::basic_string<To, std::char_traits<To>, Allocator>>(
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

    template <typename Range, typename To>
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

    template <std::size_t N>
    struct FixedString
    {
        std::array<nchar, N> data;

        // NOLINTNEXTLINE
        consteval FixedString(const char (&str)[N]) noexcept
        {
            for (usize i = 0; i < N; ++i)
            {
                data[i] = static_cast<nchar>(str[i]);
            }
        }

        // NOLINTNEXTLINE
        consteval FixedString(const wchar_t (&str)[N]) noexcept
        {
            static_assert(std::same_as<nchar, wchar_t>, "Cannot use L-literal on non-wide platform");
            for (usize i = 0; i < N; ++i)
            {
                data[i] = str[i];
            }
        }
    };

    /**
     * @brief Encapsulates a view of a C-style string or a standard library string
     * while ensuring it is null-terminated.
     *
     * @tparam T The character type of the string (e.g., char, wchar_t, etc.).
     */
    export template <Char T>
    class BasicCStringView
    {
        using ViewType = std::basic_string_view<T>;

      public:
        using traits_type = ViewType::traits_type;
        using value_type = ViewType::value_type;
        using pointer = ViewType::pointer;
        using const_pointer = ViewType::const_pointer;
        using reference = ViewType::reference;
        using const_reference = ViewType::const_reference;
        using const_iterator = ViewType::const_iterator;
        using iterator = ViewType::iterator;
        using const_reverse_iterator = ViewType::const_reverse_iterator;
        using reverse_iterator = ViewType::reverse_iterator;
        using size_type = ViewType::size_type;
        using difference_type = ViewType::difference_type;

        static constexpr size_type npos = ViewType::npos;

        template <size_type N>
        constexpr explicit(false) BasicCStringView(const T (&str)[N]) noexcept : view_{str, N - 1}
        {
            assert(str[N - 1] == '\0');
        }

        template <size_type N>
        constexpr explicit(false) BasicCStringView(const std::array<T, N> &arr) noexcept : view_{arr.data(), N - 1}
        {
            assert(arr.back() == '\0');
        }

        explicit(false) BasicCStringView(const std::basic_string<T> &str) noexcept : view_{str}
        {
        }

        explicit(false) BasicCStringView(std::basic_string<T> &&str) = delete;

        [[nodiscard]] constexpr const_iterator begin() const noexcept
        {
            return view_.begin();
        }

        [[nodiscard]] constexpr const_iterator cbegin() const noexcept
        {
            return view_.cbegin();
        }

        [[nodiscard]] constexpr const_iterator end() const noexcept
        {
            return view_.end();
        }

        [[nodiscard]] constexpr const_iterator cend() const noexcept
        {
            return view_.cend();
        }

        [[nodiscard]] constexpr const_reverse_iterator rbegin() const noexcept
        {
            return view_.rbegin();
        }

        [[nodiscard]] constexpr const_reverse_iterator crbegin() const noexcept
        {
            return view_.crbegin();
        }

        [[nodiscard]] constexpr const_reverse_iterator rend() const noexcept
        {
            return view_.rend();
        }

        [[nodiscard]] constexpr const_reverse_iterator crend() const noexcept
        {
            return view_.crend();
        }

        [[nodiscard]] constexpr const T &operator[](size_type index) const noexcept
        {
            return view_[index];
        }

        [[nodiscard]] constexpr const T &at(size_type index) const noexcept
        {
            return view_.at(index);
        }

        [[nodiscard]] constexpr const T &front() const noexcept
        {
            return view_.front();
        }

        [[nodiscard]] constexpr const T &back() const noexcept
        {
            return view_.back();
        }

        [[nodiscard]] constexpr const T *data() const noexcept
        {
            return view_.data();
        }

        [[nodiscard]] constexpr size_type size() const noexcept
        {
            return view_.size();
        }

        [[nodiscard]] constexpr size_type length() const noexcept
        {
            return view_.length();
        }

        [[nodiscard]] constexpr size_type max_size() noexcept
        {
            return view_.max_size();
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return view_.empty();
        }

        [[nodiscard]] constexpr std::basic_string_view<T> to_string_view() const noexcept
        {
            return view_;
        }

        [[nodiscard]] explicit(false) constexpr operator std::basic_string_view<T>() const noexcept
        {
            return to_string_view();
        }

        [[nodiscard]] constexpr std::basic_string<T> to_string() const noexcept
        {
            return std::basic_string<T>{view_};
        }

        [[nodiscard]] constexpr std::basic_string_view<T> remove_prefix(size_type n) const noexcept
        {
            auto view_copy = view_;
            view_copy.remove_prefix(n);
            return view_copy;
        }

        [[nodiscard]] constexpr std::basic_string_view<T> remove_suffix(size_type n) const noexcept
        {
            auto view_copy = view_;
            view_copy.remove_suffix(n);
            return view_copy;
        }

        [[nodiscard]] constexpr size_type copy(T *dest, size_type n, size_type offset = 0) const
        {
            return view_.copy(dest, n, offset);
        }

        [[nodiscard]] constexpr std::basic_string_view<T> substr(size_type offset = 0, size_type count = npos) const
        {
            return view_.substr(offset, count);
        }

        [[nodiscard]] constexpr bool starts_with(const std::basic_string_view<T> other) const noexcept
        {
            return view_.starts_with(other);
        }

        [[nodiscard]] constexpr bool starts_with(const T other) const noexcept
        {
            return view_.starts_with(other);
        }

        [[nodiscard]] constexpr bool starts_with(const T *other) const noexcept
        {
            return view_.starts_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const std::basic_string_view<T> other) const noexcept
        {
            return view_.ends_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const T other) const noexcept
        {
            return view_.ends_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const T *other) const noexcept
        {
            return view_.ends_with(other);
        }

        [[nodiscard]] constexpr bool contains(const std::basic_string_view<T> other) const noexcept
        {
            return view_.contains(other);
        }

        [[nodiscard]] constexpr bool contains(const T other) const noexcept
        {
            return view_.contains(other);
        }

        [[nodiscard]] constexpr bool contains(const T *other) const noexcept
        {
            return view_.contains(other);
        }

        [[nodiscard]] constexpr size_type find(const std::basic_string_view<T> other, size_type pos = 0) const noexcept
        {
            return view_.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T other, size_type pos = 0) const noexcept
        {
            return view_.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T *other, size_type pos = 0) const noexcept
        {
            return view_.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T *other, size_type pos, size_type count) const noexcept
        {
            return view_.find(other, pos, count);
        }

        [[nodiscard]] constexpr size_type rfind(const std::basic_string_view<T> other,
                                                size_type pos = npos) const noexcept
        {
            return view_.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T other, size_type pos = npos) const noexcept
        {
            return view_.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T *other, size_type pos = npos) const noexcept
        {
            return view_.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T *other, size_type pos, size_type count) const noexcept
        {
            return view_.rfind(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_first_of(const std::basic_string_view<T> other,
                                                        size_type pos = 0) const noexcept
        {
            return view_.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T other, size_type pos = 0) const noexcept
        {
            return view_.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T *other, size_type pos = 0) const noexcept
        {
            return view_.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T *other, size_type pos, size_type count) const noexcept
        {
            return view_.find_first_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_last_of(const std::basic_string_view<T> other,
                                                       size_type pos = npos) const noexcept
        {
            return view_.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T other, size_type pos = npos) const noexcept
        {
            return view_.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T *other, size_type pos = npos) const noexcept
        {
            return view_.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T *other, size_type pos, size_type count) const noexcept
        {
            return view_.find_last_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const std::basic_string_view<T> other,
                                                            size_type pos = 0) const noexcept
        {
            return view_.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T other, size_type pos = 0) const noexcept
        {
            return view_.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T *other, size_type pos = 0) const noexcept
        {
            return view_.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T *other,
                                                            size_type pos,
                                                            size_type count) const noexcept
        {
            return view_.find_first_not_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const std::basic_string_view<T> other,
                                                           size_type pos = npos) const noexcept
        {
            return view_.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T other, size_type pos = npos) const noexcept
        {
            return view_.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T *other, size_type pos = npos) const noexcept
        {
            return view_.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T *other,
                                                           size_type pos,
                                                           size_type count) const noexcept
        {
            return view_.find_last_not_of(other, pos, count);
        }

        [[nodiscard]] friend constexpr bool operator==(const BasicCStringView &a,
                                                       const BasicCStringView &b) noexcept = default;

        [[nodiscard]] friend constexpr bool operator==(const BasicCStringView &self,
                                                       const std::basic_string_view<T> other) noexcept
        {
            return self == other;
        }

        [[nodiscard]] friend constexpr auto operator<=>(const BasicCStringView &a, const BasicCStringView &b) noexcept
        {
            return a.view_ <=> b.view_;
        }

        [[nodiscard]] friend constexpr auto operator<=>(const BasicCStringView &self,
                                                        const std::basic_string_view<T> other) noexcept
        {
            return self.view_ <=> other;
        }

        friend auto operator<<(std::basic_ostream<T, traits_type> &stream, const BasicCStringView &view)
        {
            return stream << view.view_;
        }

      private:
        friend struct std::hash<BasicCStringView>;

        std::basic_string_view<T> view_{};
    };

    export using CStringView = BasicCStringView<char>;
    export using WCStringView = BasicCStringView<wchar_t>;
    export using U8CStringView = BasicCStringView<char8_t>;
    export using U16CStringView = BasicCStringView<char16_t>;
    export using U32CStringView = BasicCStringView<char32_t>;
    export using NCStringView = BasicCStringView<nchar>;

    export inline namespace literals
    {
        template <FixedString Str>
        consteval auto operator""_nc() noexcept
        {
            return BasicCStringView{Str.data};
        }

        consteval nchar operator""_nc(char c) noexcept
        {
            return static_cast<nchar>(c);
        }
    }
} // namespace retro

export template <>
struct std::hash<retro::CStringView>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::CStringView &view) const noexcept
    {
        return hash<string_view>{}(view.view_);
    }
};

export template <>
struct std::hash<retro::WCStringView>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::WCStringView &view) const noexcept
    {
        return hash<wstring_view>{}(view.view_);
    }
};

export template <>
struct std::hash<retro::U8CStringView>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::U8CStringView &view) const noexcept
    {
        return hash<u8string_view>{}(view.view_);
    }
};

export template <>
struct std::hash<retro::U16CStringView>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::U16CStringView &view) const noexcept
    {
        return hash<u16string_view>{}(view.view_);
    }
};

export template <>
struct std::hash<retro::U32CStringView>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::U32CStringView &view) const noexcept
    {
        return hash<u32string_view>{}(view.view_);
    }
};
