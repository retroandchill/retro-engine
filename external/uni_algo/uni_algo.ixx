/**
 * @file uni_algo.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <uni_algo/all.h>

export module uni_algo;

export namespace una
{
    namespace version
    {
        using version::library;
        using version::library_v;
        using version::unicode;
        using version::unicode_v;
    } // namespace version

    using una::utf16to32;
    using una::utf16to32u;
    using una::utf16to8;
    using una::utf16to8u;
    using una::utf32to16;
    using una::utf32to16u;
    using una::utf32to8;
    using una::utf32to8u;
    using una::utf8to16;
    using una::utf8to16u;
    using una::utf8to32;
    using una::utf8to32u;

    namespace strict
    {
        using strict::utf16to32;
        using strict::utf16to32u;
        using strict::utf16to8;
        using strict::utf16to8u;
        using strict::utf32to16;
        using strict::utf32to16u;
        using strict::utf32to8;
        using strict::utf32to8u;
        using strict::utf8to16;
        using strict::utf8to16u;
        using strict::utf8to32;
        using strict::utf8to32u;
    } // namespace strict

    using una::is_valid_utf16;
    using una::is_valid_utf32;
    using una::is_valid_utf8;

    namespace ranges
    {
        using ranges::drop_view;
        using ranges::filter_view;
        using ranges::reverse_view;
        using ranges::take_view;
        using ranges::transform_view;

        namespace views
        {
            using views::drop;
            using views::filter;
            using views::reverse;
            using views::take;
            using views::transform;
        } // namespace views
    }     // namespace ranges

    namespace views = ranges::views;

    namespace ranges
    {
        using ranges::utf16_view;
        using ranges::utf8_view;

        namespace views
        {
            using views::utf16;
            using views::utf8;
        } // namespace views

        using ranges::to_utf16;
        using ranges::to_utf16_reserve;
        using ranges::to_utf8;
        using ranges::to_utf8_reserve;
    } // namespace ranges

    namespace cases
    {
        using cases::to_casefold_utf16;
        using cases::to_casefold_utf8;
        using cases::to_lowercase_utf16;
        using cases::to_lowercase_utf8;
        using cases::to_titlecase_utf16;
        using cases::to_titlecase_utf8;
        using cases::to_uppercase_utf16;
        using cases::to_uppercase_utf8;
    } // namespace cases

    namespace casesens
    {
        using casesens::collate_utf16;
        using casesens::collate_utf8;
        using casesens::compare_utf16;
        using casesens::compare_utf8;
        using casesens::find_utf16;
        using casesens::find_utf8;
    } // namespace casesens

    namespace caseless
    {
        using caseless::collate_utf16;
        using caseless::collate_utf8;
        using caseless::compare_utf16;
        using caseless::compare_utf8;
        using caseless::find_utf16;
        using caseless::find_utf8;
    } // namespace caseless

    namespace codepoint
    {
        using codepoint::is_lowercase;
        using codepoint::is_uppercase;
        using codepoint::prop_case;
        using codepoint::to_casefold_u32;
        using codepoint::to_lowercase_u32;
        using codepoint::to_simple_casefold;
        using codepoint::to_simple_lowercase;
        using codepoint::to_simple_titlecase;
        using codepoint::to_simple_uppercase;
        using codepoint::to_titlecase_u32;
        using codepoint::to_uppercase_u32;
    } // namespace codepoint

    namespace norm
    {
        using una::norm::to_nfc_utf8;
        using una::norm::to_nfd_utf8;
        using una::norm::to_nfkc_utf8;
        using una::norm::to_nfkd_utf8;
        using una::norm::to_unaccent_utf8;

        using una::norm::to_nfc_utf16;
        using una::norm::to_nfd_utf16;
        using una::norm::to_nfkc_utf16;
        using una::norm::to_nfkd_utf16;
        using una::norm::to_unaccent_utf16;

        using una::norm::is_nfc_utf8;
        using una::norm::is_nfd_utf8;
        using una::norm::is_nfkc_utf8;
        using una::norm::is_nfkd_utf8;

        using una::norm::is_nfc_utf16;
        using una::norm::is_nfd_utf16;
        using una::norm::is_nfkc_utf16;
        using una::norm::is_nfkd_utf16;
    } // namespace norm

    namespace codepoint
    {
        using codepoint::prop_norm;

        using codepoint::to_compose;
        using codepoint::to_decompose_compat_u32;
        using codepoint::to_decompose_u32;
    } // namespace codepoint

    namespace ranges::norm
    {
        using norm::nfc_view;
        using norm::nfd_view;
        using norm::nfkc_view;
        using norm::nfkd_view;
    } // namespace ranges::norm

    namespace ranges::views::norm
    {
        using norm::nfc;
        using norm::nfd;
        using norm::nfkc;
        using norm::nfkd;
    } // namespace ranges::views::norm

    namespace codepoint
    {
        using codepoint::general_category;
        using codepoint::max_value;
        using codepoint::replacement_char;
        using codepoint::total_number;

        using codepoint::get_general_category;
        using codepoint::is_alphabetic;
        using codepoint::is_alphanumeric;
        using codepoint::is_control;
        using codepoint::is_noncharacter;
        using codepoint::is_numeric;
        using codepoint::is_private_use;
        using codepoint::is_reserved;
        using codepoint::is_supplementary;
        using codepoint::is_surrogate;
        using codepoint::is_valid;
        using codepoint::is_valid_scalar;
        using codepoint::is_whitespace;
        using codepoint::prop;

        using codepoint::get_script;
        using codepoint::has_script;
    } // namespace codepoint

    namespace ranges::grapheme
    {
        using grapheme::utf16_view;
        using grapheme::utf8_view;
    } // namespace ranges::grapheme

    namespace ranges::views::grapheme
    {
        using grapheme::utf16;
        using grapheme::utf8;
    } // namespace ranges::views::grapheme

    namespace ranges
    {
        namespace word
        {
            using word::utf16_view;
            using word::utf8_view;
        } // namespace word

        namespace word_only
        {
            using word_only::utf16_view;
            using word_only::utf8_view;
        } // namespace word_only

        namespace views
        {
            namespace word
            {
                using word::utf16;
                using word::utf8;
            } // namespace word

            namespace word_only
            {
                using word_only::utf16;
                using word_only::utf8;
            } // namespace word_only
        }     // namespace views
    }         // namespace ranges

    namespace detail::rng
    {
        using rng::operator|;
    }
} // namespace una
