//
// Created by fcors on 12/28/2025.
//
module;

#include "fmt/args.h"
#include "fmt/chrono.h"
#include "fmt/color.h"
#include "fmt/compile.h"
#include "fmt/format.h"
#include "fmt/ostream.h"
#include "fmt/printf.h"
#include "fmt/ranges.h"
// ReSharper disable once CppUnusedIncludeDirective
#include "fmt/std.h"
#include "fmt/xchar.h"

export module fmt;

export namespace fmt
{
    using fmt::dynamic_format_arg_store;

    using fmt::gmtime;
    using fmt::formatter;

    using fmt::color;
    using fmt::terminal_color;
    using fmt::emphasis;
    using fmt::rgb;
    using fmt::text_style;

    using fmt::fg;
    using fmt::bg;

    using fmt::operator|;

    using fmt::vprint;
    using fmt::print;
    using fmt::vformat;
    using fmt::format;
    using fmt::vformat_to;
    using fmt::format_to;
    using fmt::styled;

    using fmt::compiled_string;
    using fmt::is_compiled_string;

    using literals::operator ""_cf;

    using fmt::format_to_n;
    using fmt::formatted_size;

    using fmt::static_format_result;

    using detail::compile_string_to_view;
    using detail::thousands_sep_impl;
    using detail::decimal_point_impl;

    using fmt::basic_memory_buffer;
    using fmt::memory_buffer;

    using fmt::to_string;
    using fmt::writer;
    using fmt::string_buffer;
    using fmt::is_contiguous;
    using fmt::format_error;

    using fmt::basic_printf_context;
    using fmt::printf_context;
    using fmt::wprintf_context;
    using fmt::printf_args;
    using fmt::wprintf_args;

    using fmt::make_printf_args;
    using fmt::vprintf_args;
    using fmt::vsprintf;
    using fmt::sprintf;
    using fmt::vfprintf;
    using fmt::fprintf;
    using fmt::printf;

    using fmt::range_format;
    using fmt::is_tuple_like;
    using fmt::is_tuple_formattable;
    using fmt::is_range;
    using fmt::range_formatter;
    using fmt::range_format_kind;

    using fmt::join_view;
    using fmt::tuple_join_view;

    using fmt::join;

    using fmt::wstring_view;
    using fmt::wformat_parse_context;
    using fmt::wformat_context;
    using fmt::wformat_args;
    using fmt::wmemory_buffer;

    using fmt::basic_fstring;
    using fmt::basic_format_string;
    using fmt::wformat_string;
    using fmt::runtime;
    using fmt::make_wformat_args;

    using fmt::vformat_to_n;
    using fmt::format_to_n;

    using fmt::println;
    using fmt::to_wstring;
}