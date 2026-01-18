/**
 * @file name.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cstddef>

export module retro.core:strings.name;

import :defines;
import :algorithm;
import :strings.comparison;
import :strings.concepts;
import uni_algo;
import fmt;

namespace retro
{
    export class Name;

    export class RETRO_API NameEntryId
    {
      public:
        constexpr NameEntryId() = default;
        explicit constexpr NameEntryId(const uint32 value) : value_(value)
        {
        }

        [[nodiscard]] constexpr uint32 value() const noexcept
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

        friend constexpr bool operator==(const NameEntryId &lhs, const uint32 rhs) noexcept
        {
            return lhs.value_ == rhs;
        }
        friend constexpr std::strong_ordering operator<=>(const NameEntryId &lhs, const uint32 rhs) noexcept
        {
            return lhs.value_ <=> rhs;
        }

      private:
        uint32 value_ = 0;
    };
} // namespace retro

export template <>
struct std::hash<retro::NameEntryId>
{
    hash() = default;

    [[nodiscard]] constexpr usize operator()(const retro::NameEntryId id) const noexcept
    {
        return id.value();
    }
};

namespace retro
{
    export enum class NameCase : uint8
    {
        CaseSensitive,
        IgnoreCase
    };

    export enum class FindType : uint8
    {
        Find,
        Add
    };

    export RETRO_API constexpr usize MAX_NAME_LENGTH = 1024;
    constexpr usize NAME_MAX_DIGITS = 10;

    export constexpr int32 NAME_NO_NUMBER_INTERNAL = 0;

    constexpr int32 name_internal_to_external(const int32 x)
    {
        return x - 1;
    }

    constexpr int32 name_external_to_internal(const int32 x)
    {
        return x + 1;
    }

    export constexpr RETRO_API int32 NAME_NO_NUMBER = name_internal_to_external(NAME_NO_NUMBER_INTERNAL);

    template <Char CharType>
    constexpr std::pair<int32, usize> parse_number_from_name(std::basic_string_view<CharType> name)
    {
        int32 digits = 0;
        for (auto *c = name.data() + name.size() - 1; c >= name.data() && *c >= '0' && *c <= '9'; --c)
        {
            digits++;
        }

        auto *first_digit = name.data() + name.size() - digits;
        if (digits > 0 && digits < name.size() && first_digit[-1] == '_' && digits <= NAME_MAX_DIGITS &&
            (digits == 1 || *first_digit != '0'))
        {
            int64 Number = 0;
            for (int32 Index = 0; Index < digits; ++Index)
            {
                Number = 10 * Number + (first_digit[Index] - '0');
            }
            if (Number < std::numeric_limits<int32>::max())
            {
                return std::make_pair(name_external_to_internal(Number), name.size() - (1 + digits));
            }
        }

        return std::make_pair(NAME_NO_NUMBER_INTERNAL, name.size());
    }

    struct NameEntryHeader
    {
        usize length;
    };

    class alignas(alignof(std::max_align_t)) NameEntry
    {
      public:
        [[nodiscard]] constexpr usize length() const noexcept
        {
            return header_.length;
        }

        [[nodiscard]] constexpr std::string_view name() const noexcept
        {
            return std::string_view{characters_, header_.length};
        }

        [[nodiscard]] usize size_in_bytes() const noexcept;

        static consteval usize data_offset()
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
            int32 number;
        };

      public:
        constexpr Name() = default;

        explicit(false) Name(std::string_view value, FindType find_type = FindType::Add);

        explicit(false) Name(std::u16string_view value, FindType find_type = FindType::Add);

        explicit(false) Name(const std::string &value, FindType find_type = FindType::Add);

        explicit(false) Name(const std::u16string &value, FindType find_type = FindType::Add);

        explicit(false) inline Name(const char *value, const FindType find_type = FindType::Add)
            : Name(std::string_view{value}, find_type)
        {
        }

        constexpr Name(const NameEntryId comparison_index,
                       const int32 number
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

        [[nodiscard]] constexpr int32 number() const
        {
            return name_internal_to_external(number_);
        }

        constexpr void set_number(const int32 number)
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

        template <Char CharType = char, typename Allocator = std::allocator<CharType>>
        [[nodiscard]] auto to_string(Allocator allocator = Allocator{}) const
        {
            const auto baseString = get_base_string();
            if (number_ == NAME_NO_NUMBER_INTERNAL)
            {
                return convert_string<CharType>(baseString, std::move(allocator));
            }

            std::basic_string<CharType, std::char_traits<CharType>, Allocator> result{std::move(allocator)};
            fmt::format_to(std::back_inserter(result), "{}_{}", baseString, name_internal_to_external(number_));
            return result;
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
        static LookupResult lookup_name(std::u16string_view value, FindType find_type);

        NameEntryId comparison_index_;
        int32 number_ = NAME_NO_NUMBER_INTERNAL;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId display_index_;
#endif
    };

    export const RETRO_API std::vector<const NameEntry *> &debug_get_name_entries();

    export inline Name operator"" _name(const char *name, const usize length)
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
