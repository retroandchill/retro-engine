/**
 * @file text_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.text_data;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.core.localization.text_source_types;

namespace retro
{
    export class TextData : public IntrusiveRefCounted
    {
      public:
        virtual ~TextData() = default;

        virtual const std::u16string &source_string() const noexcept = 0;

        virtual const std::u16string &display_string() const noexcept = 0;

        virtual TextConstDisplayStringPtr localized_string() const noexcept = 0;

        virtual std::uint16_t global_history_revision() const noexcept = 0;

        virtual std::uint16_t local_history_revision() const noexcept = 0;
    };
} // namespace retro
