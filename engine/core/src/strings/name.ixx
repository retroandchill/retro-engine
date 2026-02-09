/**
 * @file name.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cstddef>

export module retro.core.strings.name;

import std;
import retro.core.type_traits.basic;
import retro.core.algorithm.hashing;
import retro.core.strings.format;
import retro.core.strings.encoding;
import retro.core.memory.arena_allocator;

namespace retro
{
    export class Name;

    export class RETRO_API NameEntryId
    {
      public:
        constexpr NameEntryId() = default;
        explicit constexpr NameEntryId(const std::uint32_t value) : value_(value)
        {
        }

        [[nodiscard]] constexpr std::uint32_t value() const noexcept
        {
            return value_;
        }

        [[nodiscard]] constexpr bool is_none() const noexcept
        {
            return value_ == 0;
        }

        static consteval NameEntryId none()
        {
            return {};
        }

        [[nodiscard]] std::strong_ordering compare_lexical(NameEntryId other) const noexcept;
        [[nodiscard]] std::strong_ordering compare_lexical_case_sensitive(NameEntryId other) const noexcept;

        friend constexpr bool operator==(const NameEntryId &lhs, const NameEntryId &rhs) noexcept = default;
        friend constexpr std::strong_ordering operator<=>(const NameEntryId &lhs,
                                                          const NameEntryId &rhs) noexcept = default;

        friend constexpr bool operator==(const NameEntryId &lhs, const std::uint32_t rhs) noexcept
        {
            return lhs.value_ == rhs;
        }
        friend constexpr std::strong_ordering operator<=>(const NameEntryId &lhs, const std::uint32_t rhs) noexcept
        {
            return lhs.value_ <=> rhs;
        }

      private:
        std::uint32_t value_ = 0;
    };
} // namespace retro

export template <>
struct std::hash<retro::NameEntryId>
{
    hash() = default;

    [[nodiscard]] constexpr std::size_t operator()(const retro::NameEntryId id) const noexcept
    {
        return id.value();
    }
};

namespace retro
{
    export enum class NameCase : std::uint8_t
    {
        case_sensitive,
        ignore_case
    };

    export enum class FindType : std::uint8_t
    {
        find,
        add
    };

    export RETRO_API constexpr std::size_t MAX_NAME_LENGTH = 1024;
    constexpr std::size_t NAME_MAX_DIGITS = 10;

    export constexpr std::int32_t NAME_NO_NUMBER_INTERNAL = 0;

    constexpr std::int32_t name_internal_to_external(const std::int32_t x)
    {
        return x - 1;
    }

    constexpr std::int32_t name_external_to_internal(const std::int32_t x)
    {
        return x + 1;
    }

    export constexpr RETRO_API std::int32_t NAME_NO_NUMBER = name_internal_to_external(NAME_NO_NUMBER_INTERNAL);

    template <Char CharType>
    constexpr std::pair<std::int32_t, std::size_t> parse_number_from_name(std::basic_string_view<CharType> name)
    {
        std::int32_t digits = 0;
        for (auto *c = name.data() + name.size() - 1; c >= name.data() && *c >= '0' && *c <= '9'; --c)
        {
            digits++;
        }

        auto *first_digit = name.data() + name.size() - digits;
        if (digits > 0 && digits < name.size() && first_digit[-1] == '_' && digits <= NAME_MAX_DIGITS &&
            (digits == 1 || *first_digit != '0'))
        {
            std::int64_t number = 0;
            for (std::int32_t index = 0; index < digits; ++index)
            {
                number = 10 * number + (first_digit[index] - '0');
            }
            if (number < std::numeric_limits<std::int32_t>::max())
            {
                return std::make_pair(name_external_to_internal(static_cast<std::int32_t>(number)),
                                      name.size() - (1 + digits));
            }
        }

        return std::make_pair(NAME_NO_NUMBER_INTERNAL, name.size());
    }

    struct NameEntryHeader
    {
        std::size_t length;
    };

    class alignas(alignof(std::max_align_t)) NameEntry
    {
      public:
        [[nodiscard]] constexpr std::size_t length() const noexcept
        {
            return header_.length;
        }

        [[nodiscard]] constexpr std::string_view name() const noexcept
        {
            return std::string_view{characters_, header_.length};
        }

        [[nodiscard]] std::size_t size_in_bytes() const noexcept;

        static consteval std::size_t data_offset()
        {
            return offsetof(NameEntry, data_);
        }

      private:
        friend class NameTable;

        NameEntryHeader header_{};

        // NOLINTNEXTLINE
        union
        {
            // NOLINTNEXTLINE
            char characters_[MAX_NAME_LENGTH]; // NOSONAR

            // NOLINTNEXTLINE
            std::byte data_[0]; // NOSONAR
        };
    };

    /**
     * This is the size of the stack-allocated arena for the temp strings when performing string operations.
     * Assuming sizeof(char) is 1 byte, this should allocate about 8KB of stack space, which should be support
     * multiple dynamic string resizes.
     */
    constexpr std::size_t NAME_INLINE_BUFFER_SIZE = MAX_NAME_LENGTH * 8 * sizeof(char);

    export RETRO_API constexpr std::string_view NONE_STRING = "None";

    struct NameIndices
    {
        NameEntryId comparison_index;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId display_index;
#endif
    };

    class RETRO_API Name
    {
        struct LookupResult
        {
            NameIndices indices;
            std::int32_t number;
        };

        template <EncodableRange<char>>
        struct CanImplicitlyConvert : std::false_type
        {
        };

        template <Char CharType>
        struct CanImplicitlyConvert<std::basic_string_view<CharType>> : std::true_type
        {
        };

        template <EncodableRange<char> Range>
        static constexpr bool CAN_IMPLICITLY_CONVERT = CanImplicitlyConvert<std::remove_cvref_t<Range>>::value;

      public:
        constexpr Name() = default;

        template <EncodableRange<char> Range>
        explicit(!CAN_IMPLICITLY_CONVERT<Range>) Name(Range &&value, FindType find_type = FindType::add)
            : Name(lookup_name(std::forward<Range>(value), find_type))
        {
        }

        explicit(false) inline Name(const char *value, const FindType find_type = FindType::add)
            : Name(std::string_view{value}, find_type)
        {
        }

        constexpr Name(const NameEntryId comparison_index,
                       const std::int32_t number
#if RETRO_WITH_CASE_PRESERVING_NAME
                       ,
                       const NameEntryId display_index
#endif
                       )
            : comparison_index_(comparison_index), number_(number)
#if RETRO_WITH_CASE_PRESERVING_NAME
              ,
              display_index_(display_index)
#endif
        {
        }

      private:
        inline explicit(false) Name(const LookupResult result)
            : comparison_index_(result.indices.comparison_index), number_(result.number)
#if RETRO_WITH_CASE_PRESERVING_NAME
              ,
              display_index_(result.indices.display_index)
#endif
        {
        }

      public:
        [[nodiscard]] constexpr NameEntryId comparison_index() const
        {
            return comparison_index_;
        }

        [[nodiscard]] constexpr NameEntryId display_index() const
        {
#if RETRO_WITH_CASE_PRESERVING_NAME
            return display_index_;
#else
            return comparison_index_;
#endif
        }

        [[nodiscard]] constexpr std::int32_t number() const
        {
            return name_internal_to_external(number_);
        }

        constexpr void set_number(const std::int32_t number)
        {
            number_ = name_external_to_internal(number);
        }

        constexpr static Name none()
        {
            return {};
        }

        // NOLINTNEXTLINE
        [[nodiscard]] inline bool is_valid() const
        {
            return is_within_bounds(comparison_index_);
        }

        [[nodiscard]] constexpr bool is_none() const
        {
            return comparison_index_.is_none();
        }

        template <Char CharType = char, SimpleAllocator Allocator = std::allocator<CharType>>
            requires std::same_as<CharType, typename Allocator::value_type>
        [[nodiscard]] auto to_string(Allocator allocator = Allocator{}) const
        {
            const auto baseString = get_base_string();
            if (number_ == NAME_NO_NUMBER_INTERNAL)
            {
                return convert_string<CharType>(baseString, std::move(allocator));
            }

            std::basic_string<CharType, std::char_traits<CharType>, Allocator> result{std::move(allocator)};
            format_to(std::back_inserter(result), "{}_{}", baseString, name_internal_to_external(number_));
            return result;
        }

        template <Char CharType, SimpleAllocator Allocator>
            requires std::same_as<CharType, typename Allocator::value_type>
        void append_string(std::basic_string<CharType, std::char_traits<CharType>, Allocator> &output) const
        {
            InlineArena<NAME_INLINE_BUFFER_SIZE> arena;
            output.append(to_string<CharType>(make_allocator<CharType>(arena)));
        }

        [[nodiscard]] friend constexpr bool operator==(const Name &lhs, const Name &rhs)
        {
            return lhs.comparison_index_ == rhs.comparison_index_ && lhs.number_ == rhs.number_;
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const Name &lhs, const Name &rhs)
        {
            if (const auto cmp = lhs.comparison_index_ <=> rhs.comparison_index_; cmp != std::strong_ordering::equal)
                return cmp;
            return lhs.number_ <=> rhs.number_;
        }

        [[nodiscard]] friend RETRO_API bool operator==(const Name &lhs, std::string_view rhs);

        [[nodiscard]] friend RETRO_API bool operator==(const Name &lhs, std::u16string_view rhs);

        [[nodiscard]] friend RETRO_API std::strong_ordering operator<=>(const Name &lhs, std::string_view rhs);

        [[nodiscard]] friend RETRO_API std::strong_ordering operator<=>(const Name &lhs, std::u16string_view rhs);

      private:
        [[nodiscard]] std::string_view get_base_string() const;

        static bool is_within_bounds(NameEntryId index);

        static LookupResult lookup_name(std::string_view value, FindType find_type);

        template <EncodableRange<char> Range>
        static LookupResult lookup_name(Range &&value, FindType find_type)
        {
            if constexpr (std::convertible_to<Range, std::string_view>)
            {
                return lookup_name(std::string_view{std::forward<Range>(value)}, find_type);
            }
            else if constexpr (std::ranges::contiguous_range<Range> && std::ranges::sized_range<Range> &&
                               std::same_as<std::ranges::range_value_t<Range>, char>)
            {
                return lookup_name(std::string_view{std::ranges::data(value), std::ranges::size(value)}, find_type);
            }
            else
            {
                InlineArena<NAME_INLINE_BUFFER_SIZE> arena;
                const auto utf8_str = convert_string<char>(std::forward<Range>(value), make_allocator<char>(arena));
                return lookup_name(utf8_str, find_type);
            }
        }

        NameEntryId comparison_index_;
        std::int32_t number_ = NAME_NO_NUMBER_INTERNAL;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId display_index_;
#endif
    };

    export const RETRO_API std::vector<const NameEntry *> &debug_get_name_entries();

    export inline Name operator"" _name(const char *name, const std::size_t length)
    {
        return Name{std::string_view{name, length}};
    }
} // namespace retro

export template <>
struct std::hash<retro::Name>
{
    hash() = default;

    constexpr size_t operator()(const retro::Name &name) const noexcept
    {
        return retro::hash_combine(name.comparison_index(), name.number());
    }
};
