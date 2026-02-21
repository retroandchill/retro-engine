/**
 * @file icu.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <unicode/datefmt.h>
#include <unicode/decimfmt.h>
#include <unicode/locid.h>

export module retro.core.localization.icu;

import std;

export using ::UErrorCode;

export namespace U_ICU_NAMESPACE
{
    using icu::DateFormat;
    using icu::DecimalFormat;
    using icu::DecimalFormatSymbols;
    using icu::Locale;
    using icu::NumberFormat;
    using icu::TimeZone;
    using icu::UnicodeString;
} // namespace U_ICU_NAMESPACE

export namespace icu = U_ICU_NAMESPACE;

namespace retro
{
    export inline std::int32_t write_to_output_buffer(const icu::UnicodeString &str, std::span<char16_t> buffer)
    {
        std::ranges::copy_n(str.begin(),
                            std::min(str.length(), static_cast<std::int32_t>(buffer.size())),
                            buffer.begin());
        return str.length();
    }
} // namespace retro
