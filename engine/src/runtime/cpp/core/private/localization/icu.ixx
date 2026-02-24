/**
 * @file icu.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <unicode/coll.h>
#include <unicode/datefmt.h>
#include <unicode/decimfmt.h>
#include <unicode/gregocal.h>
#include <unicode/locid.h>
#include <unicode/plurfmt.h>
#include <unicode/putil.h>
#include <unicode/udata.h>

export module retro.core.localization.icu;

export using ::UErrorCode;
export using ::UPluralType;
export using ::u_setDataDirectory;
export using ::uloc_setDefault;

export using ::UDataFileAccess;

export namespace U_ICU_NAMESPACE
{
    constexpr std::uint32_t version_major = U_ICU_VERSION_MAJOR_NUM;

    using icu::Collator;
    using icu::DateFormat;
    using icu::DecimalFormat;
    using icu::DecimalFormatSymbols;
    using icu::GregorianCalendar;
    using icu::Locale;
    using icu::NumberFormat;
    using icu::PluralRules;
    using icu::TimeZone;
    using icu::UnicodeString;
} // namespace U_ICU_NAMESPACE

export namespace icu = U_ICU_NAMESPACE;
