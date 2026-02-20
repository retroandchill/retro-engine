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

        [[nodiscard]] inline const char *three_letter_language_name() const noexcept
        {
            return locale_.getISO3Language();
        }

        [[nodiscard]] inline const char *two_letter_language_name() const noexcept
        {
            return locale_.getLanguage();
        }

        [[nodiscard]] inline const icu::UnicodeString &display_language() const noexcept
        {
            return display_language_;
        }

        [[nodiscard]] inline const char *region() const noexcept
        {
            return locale_.getCountry();
        }

        [[nodiscard]] inline const icu::UnicodeString &display_region() const noexcept
        {
            return display_region_;
        }

        [[nodiscard]] inline const char *script() const noexcept
        {
            return locale_.getScript();
        }

        [[nodiscard]] inline const icu::UnicodeString &display_script() const noexcept
        {
            return display_script_;
        }

        [[nodiscard]] inline const char *variant() const noexcept
        {
            return locale_.getVariant();
        }

        [[nodiscard]] inline const icu::UnicodeString &display_variant() const noexcept
        {
            return display_variant_;
        }

        [[nodiscard]] inline bool is_right_to_left() const noexcept
        {
            return locale_.isRightToLeft();
        }

        [[nodiscard]] inline std::int32_t lcid() const noexcept
        {
            return locale_.getLCID();
        }

      private:
        icu::Locale locale_;
        icu::UnicodeString display_name_;
        icu::UnicodeString english_name_;
        icu::UnicodeString display_language_;
        icu::UnicodeString display_script_;
        icu::UnicodeString display_region_;
        icu::UnicodeString display_variant_;
    };
} // namespace retro
