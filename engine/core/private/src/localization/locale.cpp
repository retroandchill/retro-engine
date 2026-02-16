/**
 * @file locale.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <unicode/locid.h>

module retro.core.localization.locale;

namespace retro
{
    Locale::Locale(ConstructTag, const CStringView locale_tag)
        : locale_tag_{locale_tag.empty() ? "en-US" : locale_tag.to_string()},
          icu_locale_{icu::Locale(locale_tag_.c_str())}
    {
        if (icu_locale_.isBogus())
        {
            icu_locale_ = icu::Locale("en-US");
            locale_tag_ = "en-US";
        }

        icu::UnicodeString native_name;
        icu::UnicodeString english_name;

        icu_locale_.getDisplayName(icu_locale_, native_name);
        icu_locale_.getDisplayName(icu::Locale("en-US"), english_name);

        native_name.toUTF8String(name_);
        english_name.toUTF8String(english_name_);

        auto *lang = icu_locale_.getLanguage();
        auto *region = icu_locale_.getCountry();

        language_ = lang != nullptr ? lang : "";
        region_ = region != nullptr ? region : "";
        is_right_to_left_ = icu_locale_.isRightToLeft();
    }

    std::u16string Locale::to_upper(const std::u16string_view str) const
    {
        icu::UnicodeString upper_str(str);
        upper_str.toUpper(icu_locale_);
        return std::u16string{std::from_range, upper_str};
    }

    std::u16string Locale::to_lower(const std::u16string_view str) const
    {
        icu::UnicodeString upper_str(str);
        upper_str.toLower(icu_locale_);
        return std::u16string{std::from_range, upper_str};
    }

    bool operator==(const Locale &lhs, const Locale &rhs) noexcept
    {
        return lhs.icu_locale_ == rhs.icu_locale_;
    }
} // namespace retro
