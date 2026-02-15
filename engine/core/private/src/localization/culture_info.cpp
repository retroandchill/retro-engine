/**
 * @file culture_info.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <unicode/locid.h>

module retro.core.localization.culture_info;

namespace retro
{
    CultureInfo::CultureInfo(ConstructTag, const CStringView locale_tag)
        : locale_tag_{locale_tag.empty() ? "en_US" : locale_tag.to_string()},
          icu_locale_{icu::Locale(locale_tag_.c_str())}
    {
        if (icu_locale_.isBogus())
        {
            icu_locale_ = icu::Locale("en_US");
            locale_tag_ = "en_US";
        }

        icu::UnicodeString native_name;
        icu::UnicodeString english_name;

        icu_locale_.getDisplayName(icu_locale_, native_name);
        icu_locale_.getDisplayName(icu::Locale("en_US"), english_name);

        native_name.toUTF8String(name_);
        english_name.toUTF8String(english_name_);

        auto *lang = icu_locale_.getLanguage();
        auto *region = icu_locale_.getCountry();

        language_ = lang != nullptr ? lang : "";
        region_ = region != nullptr ? region : "";
        is_right_to_left_ = icu_locale_.isRightToLeft();
    }

    bool operator==(const CultureInfo &lhs, const CultureInfo &rhs) noexcept
    {
        return lhs.icu_locale_ == rhs.icu_locale_;
    }
} // namespace retro
