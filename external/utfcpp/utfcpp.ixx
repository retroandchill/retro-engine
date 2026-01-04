/**
 * @file utfcpp.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define UTF_CPP_CPLUSPLUS 202002L
#include <utf8.h>

export module utfcpp;

export namespace utf8
{
    using utf8::exception;
    using utf8::invalid_code_point;
    using utf8::invalid_utf16;
    using utf8::invalid_utf8;
    using utf8::not_enough_room;

    using utf8::advance;
    using utf8::append;
    using utf8::append16;
    using utf8::distance;
    using utf8::iterator;
    using utf8::next;
    using utf8::next16;
    using utf8::peek_next;
    using utf8::prior;
    using utf8::replace_invalid;
    using utf8::utf16to8;
    using utf8::utf32to8;
    using utf8::utf8to16;
    using utf8::utf8to32;

    using utf8::find_invalid;
    using utf8::is_valid;
    using utf8::starts_with_bom;

    namespace unchecked
    {
        using unchecked::advance;
        using unchecked::append;
        using unchecked::append16;
        using unchecked::distance;
        using unchecked::iterator;
        using unchecked::next;
        using unchecked::next16;
        using unchecked::peek_next;
        using unchecked::prior;
        using unchecked::replace_invalid;
        using unchecked::utf16to8;
        using unchecked::utf32to8;
        using unchecked::utf8to16;
        using unchecked::utf8to32;
    } // namespace unchecked
} // namespace utf8
