/**
 * @file decimal_format_c.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"

#include <stdint.h>

#ifdef __cplusplus
extern "C"
{
#endif

    typedef struct Retro_DecimalFormat Retro_DecimalFormat;
    typedef struct Retro_Locale Retro_Locale;

    typedef struct Retro_DecimalDigits
    {
        char16_t digits[10];
    } Retro_DecimalDigits;

    typedef struct Retro_DecimalSymbol
    {
        const char16_t *buffer;
        int32_t length;
    } Retro_DecimalSymbol;

    typedef struct Retro_DecimalNumberFormattingRules
    {
        int8_t is_grouping_used;
        int32_t rounding_mode;
        int32_t minimum_integer_digits;
        int32_t maximum_integer_digits;
        int32_t minimum_fraction_digits;
        int32_t maximum_fraction_digits;
        Retro_DecimalSymbol nan_string;
        Retro_DecimalSymbol plus_string;
        Retro_DecimalSymbol minus_string;
        const char16_t grouping_seperator_char;
        const char16_t decimal_separator_char;
        const int32_t primary_grouping_size;
        const int32_t secondary_grouping_size;
        const int32_t minimum_grouping_digits;
        Retro_DecimalDigits digits;
    } RetroDecimalNumberFormattingRules;

    typedef struct Retro_DecimalFormatPrefixAndSuffixResult
    {
        int32_t positive_prefix_length;
        int32_t positive_suffix_length;
        int32_t negative_prefix_length;
        int32_t negative_suffix_length;
    } Retro_DecimalFormatPrefixAndSuffixResult;

    RETRO_API Retro_DecimalFormat *retro_create_decimal_format(const Retro_Locale *format);

    RETRO_API Retro_DecimalFormat *retro_create_percent_decimal_format(const Retro_Locale *format);

    RETRO_API Retro_DecimalFormat *retro_create_currency_decimal_format(const Retro_Locale *format);

    RETRO_API void retro_destroy_decimal_format(Retro_DecimalFormat *format);

    RETRO_API Retro_DecimalNumberFormattingRules retro_decimal_format_get_formatting_rules(Retro_DecimalFormat *format);

    RETRO_API Retro_DecimalFormatPrefixAndSuffixResult
    retro_decimal_format_get_prefix_and_suffix_lengths(Retro_DecimalFormat *format,
                                                       char16_t *positive_prefix,
                                                       int32_t positive_prefix_length,
                                                       char16_t *positive_suffix,
                                                       int32_t positive_suffix_length,
                                                       char16_t *negative_prefix,
                                                       int32_t negative_prefix_length,
                                                       char16_t *negative_suffix,
                                                       int32_t negative_suffix_length);

    RETRO_API void retro_decimal_format_set_is_grouping_used(Retro_DecimalFormat *format, int8_t is_grouping_used);

    RETRO_API void retro_decimal_format_set_currency_code(Retro_DecimalFormat *format, const char16_t *currency_code);

    RETRO_API void retro_decimal_format_set_digits(Retro_DecimalFormat *format, const Retro_DecimalDigits *digits);

#ifdef __cplusplus
}
#endif
