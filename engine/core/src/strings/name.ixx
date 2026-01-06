/**
 * @file name.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>
#include <cstddef>
#include <fmt/base.h>

#define RETRO_WITH_CASE_PRESERVING_NAME _DEBUG

export module retro.core:strings.name;

import :defines;
import :algorithm;
import :strings.comparison;
import :strings.concepts;

namespace retro
{
    export class Name;

    constexpr usize NAME_SIZE = 1024;

    export struct RETRO_API NameEntryId
    {
        constexpr NameEntryId() = default;

        explicit(false) constexpr NameEntryId(const uint32 value) : value_(value)
        {
        }

        [[nodiscard]] constexpr bool is_none() const noexcept
        {
            return value_ == 0;
        }

        [[nodiscard]] std::strong_ordering compare_lexical(NameEntryId other) const;
        [[nodiscard]] inline bool lexical_less(const NameEntryId other) const
        {
            return compare_lexical(other) < 0;
        }

        [[nodiscard]] std::strong_ordering compare_lexical_case_sensitive(NameEntryId other) const;
        [[nodiscard]] inline bool lexical_less_case_sensitive(const NameEntryId other) const
        {
            return compare_lexical_case_sensitive(other) < 0;
        }

        [[nodiscard]] constexpr std::strong_ordering compare_fast(const NameEntryId other) const
        {
            return value_ <=> other.value_;
        }

        [[nodiscard]] constexpr bool fast_less(const NameEntryId other) const
        {
            return value_ < other.value_;
        }

        [[nodiscard]] constexpr bool is_valid() const
        {
            return value_ != 0;
        }

        [[nodiscard]] constexpr uint32 to_unstable_int() const noexcept
        {
            return value_;
        }

        [[nodiscard]] constexpr static NameEntryId from_unstable_int(const uint32 value)
        {
            return {value};
        }

        constexpr friend bool operator==(const NameEntryId lhs, const NameEntryId rhs) noexcept
        {
            return lhs.to_unstable_int() == rhs.to_unstable_int();
        }

      private:
        uint32 value_ = 0;
    };
} // namespace retro

export template <>
struct std::hash<retro::NameEntryId>
{
    hash() = default;

    RETRO_API [[nodiscard]] size_t operator()(retro::NameEntryId name) const noexcept;
};

namespace retro
{
    constexpr int32 NAME_NO_NUMBER_INTERNAL = 0;

    constexpr int32 name_internal_to_external(const int32 x)
    {
        return x - 1;
    }

    constexpr int32 name_external_to_internal(const int32 x)
    {
        return x + 1;
    }

    export constexpr int32 NAME_NO_NUMBER = name_internal_to_external(NAME_NO_NUMBER_INTERNAL);

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

    template <Char CharType>
    constexpr std::pair<uint32, usize> parse_number_from_name(const std::basic_string_view<CharType> name)
    {
        int32 digits = 0;
        for (const auto *c = name.data() + name.size() - 1; c >= name.data() && *c >= '0' && *c <= '9'; --c)
        {
            ++digits;
        }

        const auto *first_digit = name.data() + name.size() - digits;
        constexpr int32 max_digits = 10;
        if (digits > 0 && digits < name.size() && first_digit[-1] == '_' && digits <= max_digits &&
            (digits == 1 || first_digit[0] != '0'))
        {
            int64 number = 0;
            for (int32 i = 0; i < digits; ++i)
            {
                number = number * 10 + (first_digit[i] - '0');
            }

            if (number < std::numeric_limits<uint32>::max())
            {
                return std::make_pair(name_external_to_internal(number), name.size() - (digits + 1));
            }

            return std::make_pair(NAME_NO_NUMBER_INTERNAL, name.size());
        }
    }

    struct NameEntryHeader
    {
        uint16 is_utf16 : 1 {};
#if RETRO_WITH_CASE_PRESERVING_NAME
        uint16 length : 15 {};
#else
        static constexpr int32 PROBE_HASH_BITS = 5;
        uint16 lowercase_probe_hash : PROBE_HASH_BITS{};
        uint16 length : 10 {};
#endif

      private:
        constexpr friend bool operator==(const NameEntryHeader lhs, const NameEntryHeader rhs) noexcept
        {
            return std::bit_cast<uint16>(lhs) == std::bit_cast<uint16>(rhs);
        }
    };

#if _DEBUG
    constexpr usize NAME_ENTRY_ALIGNMENT = 8;
#else
    constexpr usize NAME_ENTRY_ALIGNMENT = 0;
#endif

    struct alignas(NAME_ENTRY_ALIGNMENT) RETRO_API NameEntry
    {
      private:
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId comparison_id_;
#endif
        NameEntryHeader header_;

        union
        {
            std::array<char, NAME_SIZE> utf8_buffer_;
            std::array<char16_t, NAME_SIZE> utf16_buffer_;
            std::array<std::byte, 0> name_data_;
        };

        NameEntry();

      public:
        NameEntry(const NameEntry &) = delete;
        NameEntry(NameEntry &&) = delete;
        ~NameEntry() = default;
        NameEntry &operator=(const NameEntry &) = delete;
        NameEntry &operator=(NameEntry &&) = delete;

        [[nodiscard]] constexpr bool is_utf16() const noexcept
        {
            return header_.is_utf16;
        }

        [[nodiscard]] constexpr int32 name_length() const noexcept
        {
            return header_.length;
        }

        [[nodiscard]] int32 name_length_utf8() const noexcept;

        void get_unterminated_name(std::span<char16_t> out_name) const;

        void get_name(std::span<char16_t> out_name) const;

        void get_utf8_name(std::span<char> out_name) const;

        void get_utf16_name(std::span<char16_t> out_name) const;

        [[nodiscard]] std::u16string get_plain_name_string() const;

        [[nodiscard]] std::string get_plain_name_string_utf8() const;

        void append_name_to_string(std::string &out_string) const;

        void append_name_to_string(std::u16string &out_string) const;

        [[nodiscard]] usize size_in_bytes() const noexcept;

        static constexpr usize data_offset()
        {
            return offsetof(NameEntry, name_data_);
        }

        struct NameStringView make_view(union NameBuffer &optional_decode_buffer) const;

      private:
        friend class Name;
        friend struct NameHelper;
        friend class NameEntryAllocator;
        friend class NamePoolShardBase;
        friend class NamePool;

        void copy_unterminated_name(std::span<char> out_name) const;
        void copy_unterminated_name(std::span<char16_t> out_name) const;
        void copy_and_convert_unterminated_name(std::span<char16_t> out_name) const;
        const char *get_unterminated_name(std::array<char, NAME_SIZE> &optional_decode_buffer) const;
        const char16_t *get_unterminated_name(std::array<char16_t, NAME_SIZE> &optional_decode_buffer) const;
    };

    class RETRO_API Name
    {
      public:
        constexpr Name() = default;

        inline Name(const Name other, int32 number)
            : comparison_index_(other.comparison_index_), number_(number)
#if RETRO_WITH_CASE_PRESERVING_NAME
              ,
              display_index_(other.display_index_)
#endif
        {
        }

        explicit(false) Name(std::string_view name);
        explicit(false) Name(std::u16string_view name);

        [[nodiscard]] constexpr NameEntryId comparison_index() const noexcept
        {
            assert(is_within_bounds(comparison_index_));
            return comparison_index_;
        }

        [[nodiscard]] constexpr NameEntryId display_index() const noexcept
        {
            const auto index = display_index_fast();
            assert(is_within_bounds(index));
            return index;
        }

        [[nodiscard]] constexpr int32 number() const noexcept
        {
            return number_;
        }
        constexpr void set_number(const int32 number) noexcept
        {
            number_ = number;
        }

        [[nodiscard]] std::u16string plain_name_string() const;

        void get_plain_name_string(std::span<char16_t> out_name) const;
        void get_plain_name_string_utf8(std::span<char> out_name) const;
        void get_plain_name_string_utf16(std::span<char16_t> out_name) const;

        [[nodiscard]] const NameEntry *comparison_entry() const noexcept;
        [[nodiscard]] const NameEntry *display_entry() const noexcept;

        [[nodiscard]] std::string to_string() const;
        [[nodiscard]] std::u16string to_string_utf16() const;

        usize string_length() const noexcept;

        static constexpr usize STRING_BUFFER_SIZE = NAME_SIZE + 1 + 10;

        template <usize Size>
            requires(Size > STRING_BUFFER_SIZE)
        [[nodiscard]] usize to_string(char16_t (&out)[Size])
        {
            return to_string_internal(std::span{out, Size});
        }

        [[nodiscard]] inline usize to_string_truncate(const std::span<char16_t> out) const
        {
            return to_string_internal(out);
        }

        template <usize Size>
            requires(Size > 0)
        [[nodiscard]] usize to_string_truncate(char16_t (&out)[Size])
        {
            return to_string_internal(std::span{out, Size});
        }

        void append_string(std::string &out_string) const;
        void append_string(std::u16string &out_string) const;

        [[nodiscard]] inline bool is_equal(Name rhs,
                                           const NameCase compare_methods = NameCase::IgnoreCase,
                                           const bool compare_number = true) const noexcept
        {
            return ((compare_methods == NameCase::IgnoreCase) ? comparison_index() == rhs.comparison_index()
                                                              : display_index_fast() == rhs.display_index_fast()) &&
                   (!compare_number || number() == rhs.number());
        }

        inline friend bool operator==(Name lhs, Name rhs) noexcept
        {
            return lhs.to_unstable_int() == rhs.to_unstable_int();
        }

        inline bool fast_less(Name other)
        {
            return compare_indexes(other) < 0;
        }

        inline bool lexical_less(Name other)
        {
            return compare(other) < 0;
        }

        [[nodiscard]] inline bool is_none() const noexcept
        {
#if !RETRO_WITH_CASE_PRESERVING_NAME
            return to_unstable_int() != 0;
#else
            return comparison_index().is_none() && number() == NAME_NO_NUMBER;
#endif
        }

        [[nodiscard]] inline bool is_valid() const noexcept
        {
            return is_within_bounds(comparison_index_);
        }

        [[nodiscard]] std::strong_ordering compare(Name other) const noexcept;

        [[nodiscard]] inline std::strong_ordering compare_indexes(Name other) const noexcept
        {
            auto diff = comparison_index().compare_fast(other.comparison_index());
            if (diff != std::strong_ordering::equal)
            {
                return diff;
            }

            return number() <=> other.number();
        }

        [[nodiscard]] static std::u16string safe_string(NameEntryId display_index,
                                                        int32 instance_number = NAME_NO_NUMBER_INTERNAL);

        static void reserve(uint32 num_bytes, uint32 num_names);

        [[nodiscard]] static usize name_entry_memory_size();

        [[nodiscard]] static usize name_entry_memory_estimated_available();

        [[nodiscard]] static uint32 num_utf8_names();

        [[nodiscard]] static uint32 num_utf16_names();

        [[nodiscard]] static std::vector<const NameEntry *> debug_dump();

        [[nodiscard]] static const NameEntry *get_entry(NameEntryId id);

        static void auto_test();

        static void tear_down();

        [[nodiscard]] inline uint64 to_unstable_int() const noexcept
        {
            static_assert(offsetof(Name, comparison_index_) == 0);
            static_assert(offsetof(Name, number_) == 4);
            static_assert((offsetof(Name, number_) + sizeof(number_)) == sizeof(uint64));

            uint64 result = 0;
            std::memcpy(&result, this, sizeof(uint64));
            return result;
        }

      private:
        template <typename StringBufferType>
        void append_string_internal(StringBufferType &buffer);

        [[nodiscard]] constexpr NameEntryId display_index_fast() const noexcept
        {
#if RETRO_WITH_CASE_PRESERVING_NAME
            return display_index_;
#else
            return comparison_index_;
#endif
        }

        [[nodiscard]] constexpr NameEntryId comparison_index_internal() const noexcept
        {
            return comparison_index_;
        }

        [[nodiscard]] constexpr NameEntryId display_index_internal() const noexcept
        {
#if RETRO_WITH_CASE_PRESERVING_NAME
            return display_index_;
#else
            return comparison_index_;
#endif
        }

        [[nodiscard]] static const NameEntry *resolve_entry(NameEntryId lookup_id);
        [[nodiscard]] static const NameEntry *resolve_entry_recursive(NameEntryId lookup_id);

        static bool is_within_bounds(NameEntryId index) noexcept;

        [[nodiscard]] static Name create_numbered_name_if_necessary(NameEntryId comparison_id,
                                                                    NameEntryId display_id,
                                                                    int32 number)
        {
            Name out;
            out.comparison_index_ = comparison_id;
#if RETRO_WITH_CASE_PRESERVING_NAME
            out.display_index_ = display_id;
#endif
            out.number_ = NAME_NO_NUMBER_INTERNAL;
            return out;
        }

        [[nodiscard]] inline static Name create_numbered_name_if_necessary(NameEntryId comparison_id, int32 number)
        {
            return create_numbered_name_if_necessary(comparison_id, comparison_id, number);
        }

        [[nodiscard]] usize to_string_internal(std::span<char16_t> out_buffer) const;

        NameEntryId comparison_index_;
        uint32 number_ = NAME_NO_NUMBER_INTERNAL;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId display_index_;
#endif
    };

    export class RETRO_API DisplayNameEntryId
    {
      public:
        inline explicit DisplayNameEntryId(const Name name = {})
            : DisplayNameEntryId(name.display_index(), name.comparison_index())
        {
        }

        inline Name to_name(uint32 number) const
        {
            return Name{comparison_id(), display_id(), number};
        }

      private:
#if RETRO_WITH_CASE_PRESERVING_NAME
        static constexpr uint32 DIFFERENT_IDS_FLAG = 1u << 31;
        static constexpr uint32 DISPLAY_ID_MASK = ~DIFFERENT_IDS_FLAG;

        uint32 value_ = 0;

        constexpr DisplayNameEntryId(const NameEntryId id, const NameEntryId comparison_id)
            : value_(id.to_unstable_int() | (id != comparison_id) * DIFFERENT_IDS_FLAG)
        {
        }

        [[nodiscard]] constexpr bool same_ids() const
        {
            return value_ & DIFFERENT_IDS_FLAG == 0;
        }

        [[nodiscard]] inline NameEntryId display_id() const
        {
            return NameEntryId::from_unstable_int(value_ & DISPLAY_ID_MASK);
        }

        [[nodiscard]] inline NameEntryId comparison_id() const
        {
            return same_ids() ? display_id() : Name::get_comparison_id_from_display_id(display_id());
        }

      public:
        friend constexpr bool operator==(const DisplayNameEntryId lhs, const DisplayNameEntryId rhs) noexcept
        {
            return lhs.value_ == rhs.value_;
        }
#else
        NameEntryId id_;

        DisplayNameEntryId(const NameEntryId id, NameEntryId) : id_(id)
        {
        }

        constexpr NameEntryId display_id() const noexcept
        {
            return id_;
        }

        constexpr NameEntryId comparison_id() const noexcept
        {
            return id_;
        }

      public:
        constexpr friend bool operator==(const DisplayNameEntryId lhs, const DisplayNameEntryId rhs) noexcept
        {
            return lhs.id_ == rhs.id_;
        }
#endif

        constexpr friend bool operator==(const DisplayNameEntryId lhs, const NameEntryId rhs)
        {
            return lhs.display_id() == rhs;
        }

        static DisplayNameEntryId from_comparison_id(NameEntryId comparison_id);
        NameEntryId to_display_id();
        void set_loaded_comparison_id(NameEntryId comparison_id);
#if RETRO_WITH_CASE_PRESERVING_NAME
        void set_loaded_different_display_id(NameEntryId display_id);
        NameEntryId get_loaded_comparison_id() const;
#endif
    };
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
