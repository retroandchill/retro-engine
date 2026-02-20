/**
 * @file locale.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.locale;

namespace retro
{
    Locale::Locale(const char *locale) : locale_{icu::Locale::createFromName(locale)}
    {
        locale_.getDisplayName(display_name_);
        locale_.getDisplayName(icu::Locale("en"), english_name_);
    }
} // namespace retro
