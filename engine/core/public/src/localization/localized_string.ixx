/**
 * @file localized_string.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization:localized_string;

import std;
import retro.core.memory.ref_counted_ptr;
import :text_key;
import retro.core.strings.encoding;
import retro.core.util.enum_class_flags;

namespace retro
{
    export enum class LocalizedStringFlags : std::uint8_t
    {
        none = 0,
        transient = 1 << 0,
        culture_invariant = 1 << 1
    };

    export template <>
    constexpr bool is_flag_enum<LocalizedStringFlags> = true;

    export struct TextRevision
    {
        std::uint16_t global = 0; // Global text table revision
        std::uint16_t local = 0;  // Text ID-specific revision
    };

    export class LocalizedString : public IntrusiveRefCounted
    {
      public:
        virtual ~LocalizedString() = default;

        virtual const std::u16string &source_string() const noexcept = 0;

        virtual const std::u16string &display_string() const noexcept = 0;

        virtual TextRevision revision() const noexcept = 0;

        virtual TextId text_id() const noexcept
        {
            return TextId{};
        }

        virtual LocalizedStringFlags flags() const noexcept = 0;
    };

    export using LocalizedStringPtr = RefCountPtr<LocalizedString>;
    export using LocalizedStringConstPtr = RefCountPtr<const LocalizedString>;

    export RETRO_API LocalizedStringPtr
    make_unlocalized_string(std::u16string source, LocalizedStringFlags flags = LocalizedStringFlags::none);

    export template <EncodableRange<char16_t> Range>
        requires(!std::same_as<std::remove_cvref_t<Range>, std::u16string>)
    LocalizedStringPtr make_unlocalized_string(Range &&range)
    {
        return make_unlocalized_string(convert_string<char16_t>(std::forward<Range>(range)));
    }
} // namespace retro
