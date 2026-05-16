/**
 * @file decimal_format_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.localization.icu;
import retro.core.localization.buffers;

namespace retro
{
    struct DecimalDigits
    {
        char16_t digits[10]{};
    };

    struct DecimalSymbol
    {
        const char16_t *buffer = nullptr;
        std::int32_t length = 0;
    };

    struct DecimalNumberFormattingRules
    {
        std::int8_t is_grouping_used = 0;
        std::int32_t rounding_mode = 0;
        std::int32_t minimum_integer_digits = 0;
        std::int32_t maximum_integer_digits = 0;
        std::int32_t minimum_fraction_digits = 0;
        std::int32_t maximum_fraction_digits = 0;
        DecimalSymbol nan_string;
        DecimalSymbol plus_string;
        DecimalSymbol minus_string;
        const char16_t grouping_seperator_char = '\0';
        const char16_t decimal_separator_char = '\0';
        const std::int32_t primary_grouping_size = 0;
        const std::int32_t secondary_grouping_size = 0;
        const std::int32_t minimum_grouping_digits = 0;
        DecimalDigits digits;
    };

    struct DecimalFormatPrefixAndSuffixResult
    {
        std::int32_t positive_prefix_length = 0;
        std::int32_t positive_suffix_length = 0;
        std::int32_t negative_prefix_length = 0;
        std::int32_t negative_suffix_length = 0;
    };

    namespace
    {
        DecimalSymbol to_format_symbol(const icu::UnicodeString &str)
        {
            return DecimalSymbol{.buffer = str.getBuffer(), .length = str.length()};
        }
    } // namespace
} // namespace retro

extern "C"
{
    RETRO_API icu::DecimalFormat *retro_create_decimal_format(const icu::Locale *format)
    {
        UErrorCode status;
        return dynamic_cast<icu::DecimalFormat *>(icu::DecimalFormat::createInstance(*format, status));
    }

    RETRO_API icu::DecimalFormat *retro_create_percent_decimal_format(const icu::Locale *format)
    {
        UErrorCode status;
        return dynamic_cast<icu::DecimalFormat *>(icu::DecimalFormat::createPercentInstance(*format, status));
    }

    RETRO_API icu::DecimalFormat *retro_create_currency_decimal_format(const icu::Locale *format)
    {
        UErrorCode status;
        return dynamic_cast<icu::DecimalFormat *>(icu::DecimalFormat::createCurrencyInstance(*format, status));
    }

    RETRO_API void retro_destroy_decimal_format(const icu::DecimalFormat *format)
    {
        delete format;
    }

    RETRO_API const char16_t *retro_decimal_format_get_nan_symbol(const icu::DecimalFormat *format)
    {
        return format->getDecimalFormatSymbols()->getConstSymbol(icu::DecimalFormatSymbols::kNaNSymbol).getBuffer();
    }

    RETRO_API retro::DecimalNumberFormattingRules retro_decimal_format_get_formatting_rules(icu::DecimalFormat *format)
    {
        auto extract_char =
            [format](const icu::DecimalFormatSymbols::ENumberFormatSymbol symbol, const char16_t fallback_char)
        {
            auto &symbol_string = format->getDecimalFormatSymbols()->getConstSymbol(symbol);
            return symbol_string.length() > 0 ? symbol_string[0] : fallback_char;
        };

        return retro::DecimalNumberFormattingRules{
            .rounding_mode = format->getRoundingMode(),
            .minimum_integer_digits = format->getMinimumIntegerDigits(),
            .maximum_integer_digits = format->getMaximumIntegerDigits(),
            .minimum_fraction_digits = format->getMinimumFractionDigits(),
            .maximum_fraction_digits = format->getMaximumFractionDigits(),
            .nan_string = retro::to_format_symbol(
                format->getDecimalFormatSymbols()->getConstSymbol(icu::DecimalFormatSymbols::kNaNSymbol)),
            .plus_string = retro::to_format_symbol(
                format->getDecimalFormatSymbols()->getConstSymbol(icu::DecimalFormatSymbols::kPlusSignSymbol)),
            .minus_string = retro::to_format_symbol(
                format->getDecimalFormatSymbols()->getConstSymbol(icu::DecimalFormatSymbols::kMinusSignSymbol)),
            .grouping_seperator_char = extract_char(icu::DecimalFormatSymbols::kGroupingSeparatorSymbol, ','),
            .decimal_separator_char = extract_char(icu::DecimalFormatSymbols::kDecimalSeparatorSymbol, '.'),
            .primary_grouping_size = format->getGroupingSize(),
            .secondary_grouping_size = format->getSecondaryGroupingSize(),
            .minimum_grouping_digits = format->getMinimumGroupingDigits(),
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

    RETRO_API retro::DecimalFormatPrefixAndSuffixResult retro_decimal_format_get_prefix_and_suffix_lengths(
        const icu::DecimalFormat *format,
        char16_t *positive_prefix,
        const std::int32_t positive_prefix_length,
        char16_t *positive_suffix,
        const std::int32_t positive_suffix_length,
        char16_t *negative_prefix,
        const std::int32_t negative_prefix_length,
        char16_t *negative_suffix,
        const std::int32_t negative_suffix_length)
    {
        const std::span positive_prefix_span{positive_prefix, static_cast<std::size_t>(positive_prefix_length)};
        const std::span positive_suffix_span{positive_suffix, static_cast<std::size_t>(positive_suffix_length)};
        const std::span negative_prefix_span{negative_prefix, static_cast<std::size_t>(negative_prefix_length)};
        const std::span negative_suffix_span{negative_suffix, static_cast<std::size_t>(negative_suffix_length)};

        icu::UnicodeString positive_prefix_str;
        icu::UnicodeString positive_suffix_str;
        icu::UnicodeString negative_prefix_str;
        icu::UnicodeString negative_suffix_str;

        format->getPositivePrefix(positive_prefix_str);
        format->getPositiveSuffix(positive_suffix_str);
        format->getNegativePrefix(negative_prefix_str);
        format->getNegativeSuffix(negative_suffix_str);

        return retro::DecimalFormatPrefixAndSuffixResult{
            .positive_prefix_length = retro::write_to_output_buffer(positive_prefix_str, positive_prefix_span),
            .positive_suffix_length = retro::write_to_output_buffer(positive_suffix_str, positive_suffix_span),
            .negative_prefix_length = retro::write_to_output_buffer(negative_prefix_str, negative_prefix_span),
            .negative_suffix_length = retro::write_to_output_buffer(negative_suffix_str, negative_suffix_span)};
    }

    RETRO_API void retro_decimal_format_set_is_grouping_used(icu::DecimalFormat *format,
                                                             const std::int8_t is_grouping_used)
    {
        format->setGroupingUsed(is_grouping_used);
    }

    RETRO_API void retro_decimal_format_set_currency_code(icu::DecimalFormat *format, const char16_t *currency_code)
    {
        format->setCurrency(currency_code);
    }

    RETRO_API void retro_decimal_format_set_digits(icu::DecimalFormat *format, const retro::DecimalDigits *digits)
    {
        auto decimal_symbols = *format->getDecimalFormatSymbols();
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kZeroDigitSymbol, digits->digits[0], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kOneDigitSymbol, digits->digits[1], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kTwoDigitSymbol, digits->digits[2], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kThreeDigitSymbol, digits->digits[3], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kFourDigitSymbol, digits->digits[4], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kFiveDigitSymbol, digits->digits[5], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kSixDigitSymbol, digits->digits[6], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kSevenDigitSymbol, digits->digits[7], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kEightDigitSymbol, digits->digits[8], false);
        decimal_symbols.setSymbol(icu::DecimalFormatSymbols::kNineDigitSymbol, digits->digits[9], false);
        format->setDecimalFormatSymbols(decimal_symbols);
    }
}
