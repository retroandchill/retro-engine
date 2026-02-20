/**
 * @file icu.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <unicode/locid.h>

export module retro.core.localization.icu;

export namespace U_ICU_NAMESPACE
{
    using icu::Locale;
    using icu::UnicodeString;
} // namespace U_ICU_NAMESPACE

export namespace icu = U_ICU_NAMESPACE;
