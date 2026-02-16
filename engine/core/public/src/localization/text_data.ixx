/**
 * @file text_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.text:text_data;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.core.localization.text_key;
import retro.core.strings.encoding;
import retro.core.util.enum_class_flags;
import retro.core.containers.optional;

namespace retro
{
    export struct TextRevision
    {
        std::uint16_t global = 0; // Global text table revision
        std::uint16_t local = 0;  // Text ID-specific revision

        constexpr friend bool operator==(const TextRevision &lhs, const TextRevision &rhs) noexcept = default;
    };

    export using TextDisplayStringPtr = std::shared_ptr<std::u16string>;
    export using TextDisplayStringConstPtr = std::shared_ptr<const std::u16string>;

    class TextData : public IntrusiveRefCounted
    {
      public:
        virtual ~TextData() = default;

        virtual const std::u16string &source_string() const noexcept = 0;

        virtual const std::u16string &display_string() const noexcept = 0;

        virtual TextDisplayStringConstPtr localized_string() const noexcept = 0;

        virtual TextRevision revision() const noexcept = 0;

        virtual class TextHistory &mutable_text_history() noexcept = 0;

        virtual const TextHistory &text_history() const noexcept = 0;
    };
} // namespace retro
