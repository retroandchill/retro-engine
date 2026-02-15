
/**
 * @file guid.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <boost/uuid.hpp>

export module retro.core.util.guid;

import std;
import retro.core.type_traits.basic;
import retro.core.type_traits.range;
import retro.core.containers.optional;

namespace retro
{
    template <Char T>
    [[nodiscard]] constexpr boost::uuids::uuid::value_type hex_to_byte(const T ch) noexcept
    {
        if (ch >= static_cast<T>('0') && ch <= static_cast<T>('9'))
            return static_cast<boost::uuids::uuid::value_type>(ch - static_cast<T>('0'));
        if (ch >= static_cast<T>('a') && ch <= static_cast<T>('f'))
            return static_cast<boost::uuids::uuid::value_type>(10 + ch - static_cast<T>('a'));
        if (ch >= static_cast<T>('A') && ch <= static_cast<T>('F'))
            return static_cast<boost::uuids::uuid::value_type>(10 + ch - static_cast<T>('A'));
        return 0;
    }

    template <Char T>
    [[nodiscard]] constexpr bool is_hex(const T ch)
    {
        return (ch >= static_cast<T>('0') && ch <= static_cast<T>('9')) ||
               (ch >= static_cast<T>('a') && ch <= static_cast<T>('f')) ||
               (ch >= static_cast<T>('A') && ch <= static_cast<T>('F'));
    }

    export enum class GuidVariant : std::uint8_t
    {
        ncs = boost::uuids::uuid::variant_ncs,
        rfc = boost::uuids::uuid::variant_rfc_4122,
        microsoft = boost::uuids::uuid::variant_microsoft,
        reserved = boost::uuids::uuid::variant_future
    };

    export enum class GuidVersion : std::int8_t
    {
        unknown = boost::uuids::uuid::version_unknown,
        time_based = boost::uuids::uuid::version_time_based,
        dce_security = boost::uuids::uuid::version_dce_security,
        name_based_md5 = boost::uuids::uuid::version_name_based_md5,
        random_number_based = boost::uuids::uuid::version_random_number_based,
        name_based_sha1 = boost::uuids::uuid::version_name_based_sha1,
        time_based_v6 = boost::uuids::uuid::version_time_based_v6,
        time_based_v7 = boost::uuids::uuid::version_time_based_v7,
        custom_v8 = boost::uuids::uuid::version_custom_v8
    };

    export enum class GuidFormat : std::uint8_t
    {
        digits,
        digits_lower,
        digits_with_hyphens,
        digits_with_hyphens_lower,
        digits_with_hyphens_in_braces,
        digits_with_hyphens_in_parentheses,
        hex_values_in_braces
    };

    /**
     * @brief A GUID (Globally Unique Identifier) type compatible with C# System.Guid
     *
     * Represents a 128-bit universally unique identifier in RFC 4122 format.
     * The memory layout is compatible with C# System.Guid for seamless interop.
     */
    export class RETRO_API Guid
    {
        using DataType = boost::uuids::uuid;

      public:
        using value_type = DataType::value_type;
        using reference = DataType::reference;
        using const_reference = DataType::const_reference;
        using iterator = DataType::iterator;
        using const_iterator = DataType::const_iterator;
        using size_type = DataType::size_type;
        using difference_type = DataType::difference_type;

        constexpr Guid() = default;

        constexpr explicit Guid(std::span<const value_type> arr) noexcept
        {
            std::ranges::copy(arr, data_.begin());
        }

        template <std::forward_iterator Iterator>
            requires std::convertible_to<typename std::iterator_traits<Iterator>::value_type, value_type>
        explicit Guid(Iterator first, Iterator last) noexcept
        {
            if (std::distance(first, last) != 16)
                throw std::length_error{"Invalid GUID length"};

            std::ranges::copy(first, last, data_.begin());
        }

        [[nodiscard]] constexpr iterator begin() noexcept
        {
            return data_.begin();
        }

        [[nodiscard]] constexpr const_iterator begin() const noexcept
        {
            return data_.begin();
        }

        [[nodiscard]] constexpr iterator end() noexcept
        {
            return data_.end();
        }

        [[nodiscard]] constexpr const_iterator end() const noexcept
        {
            return data_.end();
        }

        [[nodiscard]] constexpr value_type *data() noexcept
        {
            return data_.data();
        }

        [[nodiscard]] constexpr const value_type *data() const noexcept
        {
            return data_.data();
        }

        [[nodiscard]] static constexpr size_type size() noexcept
        {
            return DataType::static_size();
        }

        [[nodiscard]] inline GuidVariant variant() const noexcept
        {
            return static_cast<GuidVariant>(data_.variant());
        }

        [[nodiscard]] inline GuidVersion version() const noexcept
        {
            return static_cast<GuidVersion>(data_.version());
        }

        [[nodiscard]] inline bool is_nil() const noexcept
        {
            return data_.is_nil();
        }

        inline void swap(Guid &other) noexcept
        {
            return data_.swap(other.data_);
        }

        [[nodiscard]] inline std::span<const std::byte, 16> as_bytes() const
        {
            return std::as_bytes(std::span<const value_type, 16>{data_});
        }

        static constexpr Guid nil() noexcept
        {
            return Guid{};
        }

        static Guid create();

        static Guid create_v7();

        template <std::ranges::forward_range StringType>
            requires Char<std::ranges::range_value_t<StringType>> && std::ranges::sized_range<StringType>
        [[nodiscard]] constexpr static Optional<Guid> parse(StringType &&range)
        {
            switch (std::ranges::size(range))
            {
                case 32:
                    return parse_exact(range, GuidFormat::digits);
                case 36:
                    return parse_exact(range, GuidFormat::digits_with_hyphens);
                case 38:
                    if (*std::ranges::begin(range) == '{')
                    {
                        return parse_exact(range, GuidFormat::digits_with_hyphens_in_braces);
                    }

                    return parse_exact(range, GuidFormat::digits_with_hyphens_in_parentheses);
                case 68:
                    return parse_exact(range, GuidFormat::hex_values_in_braces);
                default:
                    return std::nullopt;
            }
        }

        template <std::ranges::forward_range StringType>
            requires Char<std::ranges::range_value_t<StringType>> && std::ranges::sized_range<StringType>
        [[nodiscard]] constexpr static Optional<Guid> parse_exact(StringType &&range, GuidFormat format)
        {
            std::array<value_type, 16> bytes{};
            auto it = std::ranges::begin(range);
            auto end = std::ranges::end(range);

            // Skip opening brace if present
            if (format == GuidFormat::digits_with_hyphens_in_braces ||
                format == GuidFormat::digits_with_hyphens_in_parentheses || format == GuidFormat::hex_values_in_braces)
            {
                if (it == end)
                    return std::nullopt;
                ++it; // Skip '{' or '('
            }

            std::size_t byte_index = 0;

            while (it != end && byte_index < 16)
            {
                auto ch = *it;

                // Skip hyphens if expected
                if ((format == GuidFormat::digits_with_hyphens || format == GuidFormat::digits_with_hyphens_in_braces ||
                     format == GuidFormat::digits_with_hyphens_in_parentheses) &&
                    ch == static_cast<std::ranges::range_value_t<StringType>>('-'))
                {
                    ++it;
                    continue;
                }

                // Skip spaces if hex_values_in_braces format
                if (format == GuidFormat::hex_values_in_braces)
                {
                    if (ch == static_cast<std::ranges::range_value_t<StringType>>(' ') ||
                        ch == static_cast<std::ranges::range_value_t<StringType>>('{') ||
                        ch == static_cast<std::ranges::range_value_t<StringType>>('}') ||
                        ch == static_cast<std::ranges::range_value_t<StringType>>(','))
                    {
                        ++it;
                        continue;
                    }
                }

                if (!is_hex(ch))
                    return std::nullopt;

                auto hi = hex_to_byte(ch);
                ++it;

                if (it == end)
                    return std::nullopt;

                ch = *it;

                // Skip hyphens again if needed
                if ((format == GuidFormat::digits_with_hyphens || format == GuidFormat::digits_with_hyphens_in_braces ||
                     format == GuidFormat::digits_with_hyphens_in_parentheses) &&
                    ch == static_cast<typename std::ranges::range_value_t<StringType>>('-'))
                {
                    --it; // Back up to reprocess the second hex digit
                    ++it;
                    ++it;
                    continue;
                }

                if (!is_hex(ch))
                    return std::nullopt;

                auto lo = hex_to_byte(ch);
                bytes[byte_index++] = static_cast<value_type>((hi << 4) | lo);
                ++it;
            }

            // Skip closing brace if present
            if (format == GuidFormat::digits_with_hyphens_in_braces ||
                format == GuidFormat::digits_with_hyphens_in_parentheses || format == GuidFormat::hex_values_in_braces)
            {
                if (it == end)
                    return std::nullopt;
                ++it; // Skip '}' or ')'
            }

            // Verify we've consumed the entire range
            if (it != end || byte_index != 16)
                return std::nullopt;

            return Guid{bytes};
        }

        template <Char T = char, SimpleAllocator Allocator = std::allocator<T>>
        constexpr std::basic_string<T, std::char_traits<T>, Allocator> to_string() const
        {
            constexpr std::size_t size = 36;
            std::basic_string<T, std::char_traits<T>, Allocator> str{};
            str.reserve(size);

            append_string(str);
            return str;
        }

        template <Char T, CharTraits Traits, SimpleAllocator Allocator>
        constexpr void append_string(std::basic_string<T, Traits, Allocator> &str) const
        {
            boost::uuids::to_chars(data_, std::back_inserter(str));
        }

      private:
        explicit Guid(DataType data) noexcept : data_{data}
        {
        }

        DataType data_{};

        inline friend bool operator==(const Guid &lhs, const Guid &rhs) noexcept
        {
            return lhs.data_ == rhs.data_;
        }

        inline friend std::strong_ordering operator<=>(const Guid &lhs, const Guid &rhs) noexcept
        {
            return lhs.data_ <=> rhs.data_;
        }

        template <Char Elem, typename Traits>
        friend std::basic_ostream<Elem, Traits> &operator<<(std::basic_ostream<Elem, Traits> &os, const Guid &guid)
        {
            return os << guid.data_;
        }

        template <Char Elem, typename Traits>
        friend std::basic_ostream<Elem, Traits> &operator>>(std::basic_ostream<Elem, Traits> &os, const Guid &guid)
        {
            return os >> guid.data_;
        }

        friend std::hash<Guid>;
    };

} // namespace retro

export template <>
struct std::hash<retro::Guid>
{
    inline std::size_t operator()(const retro::Guid &guid) const noexcept
    {
        return std::hash<retro::Guid::DataType>{}(guid.data_);
    }
};
