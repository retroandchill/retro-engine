/**
 * @file locale.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.locale;

import std;
import retro.core.localization.icu;

namespace retro
{
    export class Locale
    {
      public:
        explicit Locale(const char *locale);

        [[nodiscard]] inline bool is_bogus() const noexcept
        {
            return locale_.isBogus();
        }

        [[nodiscard]] inline const char *name() const noexcept
        {
            return locale_.getName();
        }

        [[nodiscard]] inline const icu::UnicodeString &display_name() const noexcept
        {
            return display_name_;
        }

        [[nodiscard]] inline const icu::UnicodeString &english_name() const noexcept
        {
            return english_name_;
        }

      private:
        icu::Locale locale_;
        icu::UnicodeString display_name_;
        icu::UnicodeString english_name_;
    };
} // namespace retro
