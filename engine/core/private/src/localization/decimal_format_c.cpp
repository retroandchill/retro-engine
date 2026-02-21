/**
 * @file decimal_format_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/decimal_format_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale)
DECLARE_OPAQUE_C_HANDLE(Retro_DecimalFormat, icu::DecimalFormat)

Retro_DecimalFormat *retro_create_decimal_format(const Retro_Locale *format)
{
    UErrorCode status;
    return retro::to_c(
        static_cast<icu::DecimalFormat *>(icu::NumberFormat::createInstance(*retro::from_c(format), status)));
}

Retro_DecimalFormat *retro_create_percent_decimal_format(const Retro_Locale *format)
{
    UErrorCode status;
    return retro::to_c(
        static_cast<icu::DecimalFormat *>(icu::NumberFormat::createPercentInstance(*retro::from_c(format), status)));
}

Retro_DecimalFormat *retro_create_currency_decimal_format(const Retro_Locale *format)
{
    UErrorCode status;
    return retro::to_c(
        static_cast<icu::DecimalFormat *>(icu::NumberFormat::createCurrencyInstance(*retro::from_c(format), status)));
}

void retro_destroy_decimal_format(Retro_DecimalFormat *format)
{
    delete retro::from_c(format);
}

const char16_t *retro_decimal_format_get_nan_symbol(Retro_DecimalFormat *format)
{
    return retro::from_c(format)
        ->getDecimalFormatSymbols()
        ->getConstSymbol(icu::DecimalFormatSymbols::kNaNSymbol)
        .getBuffer();
}

Retro_DecimalNumberFormattingRules retro_decimal_format_get_formatting_rules(Retro_DecimalFormat *format)
{
    auto *cpp_format = retro::from_c(format);

    auto extract_char =
        [cpp_format](const icu::DecimalFormatSymbols::ENumberFormatSymbol symbol, const char16_t fallback_char)
    {
        auto &symbol_string = cpp_format->getDecimalFormatSymbols()->getConstSymbol(symbol);
        return symbol_string.length() > 0 ? symbol_string[0] : fallback_char;
    };

    return Retro_DecimalNumberFormattingRules{
        .rounding_mode = cpp_format->getRoundingMode(),
        .minimum_integer_digits = cpp_format->getMinimumIntegerDigits(),
        .maximum_integer_digits = cpp_format->getMaximumIntegerDigits(),
        .minimum_fraction_digits = cpp_format->getMinimumFractionDigits(),
        .maximum_fraction_digits = cpp_format->getMaximumFractionDigits(),
        .nan_string =
            cpp_format->getDecimalFormatSymbols()->getConstSymbol(icu::DecimalFormatSymbols::kNaNSymbol).getBuffer(),
        .plus_string = cpp_format->getDecimalFormatSymbols()
                           ->getConstSymbol(icu::DecimalFormatSymbols::kPlusSignSymbol)
                           .getBuffer(),
        .minus_string = cpp_format->getDecimalFormatSymbols()
                            ->getConstSymbol(icu::DecimalFormatSymbols::kMinusSignSymbol)
                            .getBuffer(),
        .grouping_seperator_char = extract_char(icu::DecimalFormatSymbols::kGroupingSeparatorSymbol, ','),
        .decimal_separator_char = extract_char(icu::DecimalFormatSymbols::kDecimalSeparatorSymbol, '.'),
        .primary_grouping_size = cpp_format->getGroupingSize(),
        .secondary_grouping_size = cpp_format->getSecondaryGroupingSize(),
        .minimum_grouping_digits = cpp_format->getMinimumGroupingDigits(),
        .digits = {extract_char(icu::DecimalFormatSymbols::kZeroDigitSymbol, '0'),
                   extract_char(icu::DecimalFormatSymbols::kOneDigitSymbol, '1'),
                   extract_char(icu::DecimalFormatSymbols::kTwoDigitSymbol, '2'),
                   extract_char(icu::DecimalFormatSymbols::kThreeDigitSymbol, '3'),
                   extract_char(icu::DecimalFormatSymbols::kFourDigitSymbol, '4'),
                   extract_char(icu::DecimalFormatSymbols::kFiveDigitSymbol, '5'),
                   extract_char(icu::DecimalFormatSymbols::kSixDigitSymbol, '6'),
                   extract_char(icu::DecimalFormatSymbols::kSevenDigitSymbol, '7'),
                   extract_char(icu::DecimalFormatSymbols::kEightDigitSymbol, '8'),
                   extract_char(icu::DecimalFormatSymbols::kNineDigitSymbol, '9')}};
}

Retro_DecimalFormatPrefixAndSuffixResult retro_decimal_format_get_prefix_and_suffix_lengths(
    Retro_DecimalFormat *format,
    char16_t *positive_prefix,
    int32_t positive_prefix_length,
    char16_t *positive_suffix,
    int32_t positive_suffix_length,
    char16_t *negative_prefix,
    int32_t negative_prefix_length,
    char16_t *negative_suffix,
    int32_t negative_suffix_length)
{
    auto *cpp_format = retro::from_c(format);

    std::span positive_prefix_span{positive_prefix, static_cast<std::size_t>(positive_prefix_length)};
    std::span positive_suffix_span{positive_suffix, static_cast<std::size_t>(positive_suffix_length)};
    std::span negative_prefix_span{negative_prefix, static_cast<std::size_t>(negative_prefix_length)};
    std::span negative_suffix_span{negative_suffix, static_cast<std::size_t>(negative_suffix_length)};

    icu::UnicodeString positive_prefix_str;
    icu::UnicodeString positive_suffix_str;
    icu::UnicodeString negative_prefix_str;
    icu::UnicodeString negative_suffix_str;

    cpp_format->getPositivePrefix(positive_prefix_str);
    cpp_format->getPositiveSuffix(positive_suffix_str);
    cpp_format->getNegativePrefix(negative_prefix_str);
    cpp_format->getNegativeSuffix(negative_suffix_str);

    auto get_string_value = [](const icu::UnicodeString &str, std::span<char16_t> buffer)
    {
        std::ranges::copy_n(str.begin(),
                            std::min(str.length(), static_cast<std::int32_t>(buffer.size())),
                            buffer.begin());
        return str.length();
    };

    return Retro_DecimalFormatPrefixAndSuffixResult{
        .positive_prefix_length = get_string_value(positive_prefix_str, positive_prefix_span),
        .positive_suffix_length = get_string_value(positive_suffix_str, positive_suffix_span),
        .negative_prefix_length = get_string_value(negative_prefix_str, negative_prefix_span),
        .negative_suffix_length = get_string_value(negative_suffix_str, negative_suffix_span)};
}

void retro_decimal_format_set_is_grouping_used(Retro_DecimalFormat *format, const int8_t is_grouping_used)
{
    retro::from_c(format)->setGroupingUsed(is_grouping_used);
}

void retro_decimal_format_set_currency_code(Retro_DecimalFormat *format, const char16_t *currency_code)
{
    retro::from_c(format)->setCurrency(currency_code);
}

void retro_decimal_format_set_digits(Retro_DecimalFormat *format, const Retro_DecimalDigits *digits)
{
    auto *cpp_format = retro::from_c(format);

    auto decimal_symbols = *cpp_format->getDecimalFormatSymbols();
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kZeroDigitSymbol, digits->digits[0]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kOneDigitSymbol, digits->digits[1]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kTwoDigitSymbol, digits->digits[2]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kThreeDigitSymbol, digits->digits[3]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kFourDigitSymbol, digits->digits[4]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kFiveDigitSymbol, digits->digits[5]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kSixDigitSymbol, digits->digits[6]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kSevenDigitSymbol, digits->digits[7]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kEightDigitSymbol, digits->digits[8]);
    decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kNineDigitSymbol, digits->digits[9]);
    cpp_format->setDecimalFormatSymbols(decimal_symbols);
}
