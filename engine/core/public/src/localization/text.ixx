/**
 * @file text.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization.text;

import std;
import retro.core.containers.optional;
import retro.core.util.enum_class_flags;
export import :text_data;
import retro.core.memory.ref_counted_ptr;
import retro.core.localization.text_key;
import retro.core.util.date_time;
import retro.core.strings.name;
import retro.core.functional.delegate;
import retro.core.type_traits.basic;
import retro.core.strings.encoding;

namespace retro
{
    export enum class TextFlag : std::uint32_t
    {
        none = 0,
        transient = 1 << 0,
        culture_invariant = 1 << 1,
        converted_property = 1 << 2,
        immutable = 1 << 3,
        initialized_from_string = 1 << 4
    };

    export template <>
    constexpr bool is_flag_enum<TextFlag> = true;

    export enum class TextIdenticalModeFlags : std::uint8_t
    {
        none = 0,
        deep_compare = 1 << 0,
        lexical_compare_invariants = 1 << 1
    };

    export template <>
    constexpr bool is_flag_enum<TextIdenticalModeFlags> = true;

    export enum class TextComparisonLevel
    {
        default_compare,
        primary,
        secondary,
        tertiary,
        quaternary,
        quinary
    };

    export class RETRO_API Text final
    {
        template <EncodableRange<char>>
        struct CanImplicitlyConvert : std::false_type
        {
        };

        template <Char CharType>
        struct CanImplicitlyConvert<std::basic_string_view<CharType>> : std::true_type
        {
        };

        template <EncodableRange<char16_t> Range>
        static constexpr bool can_implicitly_convert = CanImplicitlyConvert<std::remove_cvref_t<Range>>::value;

      public:
        static const Text &empty();

        Text();

        explicit Text(std::u16string &&source_string);

        template <EncodableRange<char16_t> Range>
        explicit(can_implicitly_convert<Range>) Text(Range &&range)
            : Text(convert_string<char16_t>(std::forward<Range>(range)))
        {
        }

        Text(const Text &) = default;
        Text(Text &&other) noexcept;

        ~Text() = default;

        Text &operator=(const Text &) = default;
        Text &operator=(Text &&other) noexcept;

        static Text from_name(Name value);

        template <EncodableRange<char16_t> Range>
        static Text as_culture_invariant(Range &&range)
        {
            return as_culture_invariant(convert_string<char16_t>(std::forward<Range>(range)));
        }

        static Text as_culture_invariant(std::u16string &&source_string);

        static Text as_culture_invariant(Text text);

        [[nodiscard]] const std::u16string &to_string() const noexcept;

        [[nodiscard]] std::strong_ordering compare_to(
            const Text &other,
            TextComparisonLevel comparison_level = TextComparisonLevel::default_compare) const;
        [[nodiscard]] std::strong_ordering compare_to_ignore_case(const Text &other) const noexcept;

        [[nodiscard]] bool equals(
            const Text &other,
            TextComparisonLevel comparison_level = TextComparisonLevel::default_compare) const noexcept;
        [[nodiscard]] bool equals_ignore_case(const Text &other) const noexcept;

        [[nodiscard]] inline bool friend operator==(const Text &lhs, const Text &rhs) noexcept
        {
            return lhs.equals(rhs);
        }

        [[nodiscard]] inline std::strong_ordering friend operator<=>(const Text &lhs, const Text &rhs) noexcept
        {
            return lhs.compare_to(rhs);
        }

        [[nodiscard]] bool is_empty() const noexcept;

        [[nodiscard]] bool is_empty_or_whitespace() const noexcept;

        [[nodiscard]] Text to_lower() const;

        [[nodiscard]] Text to_upper() const;

        [[nodiscard]] bool is_transient() const noexcept;
        [[nodiscard]] bool is_culture_invariant() const noexcept;
        [[nodiscard]] bool is_initialized_from_string() const noexcept;

      private:
        template <std::derived_from<TextData> T>
        explicit Text(RefCountPtr<T> text_data, const TextFlag flags = TextFlag::none)
            : text_data_(std::move(text_data)), flags_{flags}
        {
        }

        void rebuild() const;

        RefCountPtr<TextData> text_data_;
        TextFlag flags_ = TextFlag::none;

        friend class TextHistoryTransformed;
    };
} // namespace retro
