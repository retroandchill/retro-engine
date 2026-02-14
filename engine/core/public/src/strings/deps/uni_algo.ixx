/**
 * @file uni_algo.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <uni_algo/all.h>

export module retro.core.strings.encoding:uni_algo;

export namespace una
{
    using una::utf16to32;
    using una::utf16to8;
    using una::utf32to16;
    using una::utf32to8;
    using una::utf8to16;
    using una::utf8to32;

    namespace ranges
    {
        using ranges::to_utf16;
        using ranges::to_utf8;

        namespace views
        {
            using views::utf16;
            using views::utf8;
        } // namespace views
    }     // namespace ranges

    namespace views = ranges::views;

    namespace cases
    {
        using cases::to_lowercase_utf16;
        using cases::to_lowercase_utf8;
        using cases::to_titlecase_utf16;
        using cases::to_titlecase_utf8;
        using cases::to_uppercase_utf16;
        using cases::to_uppercase_utf8;
    } // namespace cases

    namespace casesens
    {
        using casesens::compare_utf16;
        using casesens::compare_utf8;
    } // namespace casesens

    namespace caseless
    {
        using caseless::compare_utf16;
        using caseless::compare_utf8;
    } // namespace caseless
} // namespace una
