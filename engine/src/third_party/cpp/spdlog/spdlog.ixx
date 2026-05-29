/**
 * @file spdlog.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <spdlog/async.h>
#include <spdlog/cfg/argv.h>
#include <spdlog/cfg/env.h>
#include <spdlog/cfg/helpers.h>
#include <spdlog/mdc.h>
#include <spdlog/pattern_formatter.h>
#include <spdlog/sinks/android_sink.h>
#include <spdlog/sinks/base_sink.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/callback_sink.h>
#include <spdlog/sinks/daily_file_sink.h>
#include <spdlog/sinks/dist_sink.h>
#include <spdlog/sinks/dup_filter_sink.h>
#include <spdlog/sinks/hourly_file_sink.h>
#include <spdlog/sinks/null_sink.h>
#include <spdlog/sinks/ostream_sink.h>
#include <spdlog/sinks/ringbuffer_sink.h>
#include <spdlog/sinks/rotating_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/sinks/stdout_sinks.h>
#include <spdlog/sinks/tcp_sink.h>
#include <spdlog/sinks/udp_sink.h>
#include <spdlog/spdlog.h>
#include <spdlog/stopwatch.h>

#ifdef _WIN32
#include <spdlog/sinks/msvc_sink.h>
#include <spdlog/sinks/win_eventlog_sink.h>
#endif

export module spdlog;

export namespace spdlog
{
    using spdlog::err_handler;
    using spdlog::filename_t;
    using spdlog::log_clock;
    using spdlog::sink_ptr;
    using spdlog::sinks_init_list;

    using namespace spdlog::fmt_lib;
    using spdlog::format_string_t;
    using spdlog::is_convertible_to_any_format_string;
    using spdlog::is_convertible_to_basic_format_string;
    using spdlog::level_t;
    using spdlog::memory_buf_t;
    using spdlog::string_view_t;

    namespace level
    {
        using level::level_enum;

        using level::from_str;
        using level::to_short_c_str;
        using level::to_string_view;
    } // namespace level

    using spdlog::color_mode;
    using spdlog::pattern_time_type;

    using spdlog::file_event_handlers;
    using spdlog::source_loc;

    using spdlog::logger;
    using spdlog::swap;

    using spdlog::spdlog_ex;
    using spdlog::throw_spdlog_ex;

    using spdlog::apply_all;
    using spdlog::apply_logger_env_levels;
    using spdlog::create;
    using spdlog::default_factory;
    using spdlog::default_logger;
    using spdlog::default_logger_raw;
    using spdlog::disable_backtrace;
    using spdlog::drop;
    using spdlog::drop_all;
    using spdlog::dump_backtrace;
    using spdlog::enable_backtrace;
    using spdlog::flush_every;
    using spdlog::flush_on;
    using spdlog::get;
    using spdlog::get_level;
    using spdlog::initialize_logger;
    using spdlog::register_logger;
    using spdlog::register_or_replace;
    using spdlog::set_automatic_registration;
    using spdlog::set_default_logger;
    using spdlog::set_error_handler;
    using spdlog::set_formatter;
    using spdlog::set_level;
    using spdlog::set_pattern;
    using spdlog::should_log;
    using spdlog::shutdown;

    using spdlog::critical;
    using spdlog::debug;
    using spdlog::error;
    using spdlog::info;
    using spdlog::log;
    using spdlog::trace;
    using spdlog::warn;

    using spdlog::async_factory;
    using spdlog::async_factory_impl;
    using spdlog::async_factory_nonblock;
    using spdlog::async_logger;
    using spdlog::async_overflow_policy;
    using spdlog::create_async;
    using spdlog::create_async_nb;
    using spdlog::init_thread_pool;
    using spdlog::thread_pool;

    using spdlog::formatter;
    using spdlog::mdc;

    using spdlog::custom_flag_formatter;
    using spdlog::pattern_formatter;

    using spdlog::stopwatch;

    namespace cfg
    {
        using cfg::load_argv_levels;
        using cfg::load_env_levels;

        namespace helpers
        {
            using helpers::load_levels;
        }
    } // namespace cfg

    using spdlog::custom_log_callback;

    namespace sinks
    {
        using sinks::base_sink;
        using sinks::sink;

#ifdef __ANDROID__
        using sinks::android_sink;
        using sinks::android_sink_buf_mt;
        using sinks::android_sink_buf_st;
        using sinks::android_sink_mt;
        using sinks::android_sink_st;
#endif

        using sinks::basic_file_sink;
        using sinks::basic_file_sink_mt;
        using sinks::basic_file_sink_st;

        using sinks::callback_sink;
        using sinks::callback_sink_mt;
        using sinks::callback_sink_st;

        using sinks::daily_file_format_sink_mt;
        using sinks::daily_file_format_sink_st;
        using sinks::daily_file_sink;
        using sinks::daily_file_sink_mt;
        using sinks::daily_file_sink_st;
        using sinks::daily_filename_calculator;
        using sinks::daily_filename_format_calculator;

        using sinks::dist_sink;
        using sinks::dist_sink_mt;
        using sinks::dist_sink_st;

        using sinks::dup_filter_sink;
        using sinks::dup_filter_sink_mt;
        using sinks::dup_filter_sink_st;

        using sinks::hourly_file_sink;
        using sinks::hourly_file_sink_mt;
        using sinks::hourly_file_sink_st;
        using sinks::hourly_filename_calculator;

#ifdef _WIN32
        using sinks::msvc_sink;
        using sinks::msvc_sink_mt;
        using sinks::msvc_sink_st;
        using sinks::windebug_sink_mt;
        using sinks::windebug_sink_st;
#endif

        using sinks::null_sink;
        using sinks::null_sink_mt;
        using sinks::null_sink_st;

        using sinks::ostream_sink;
        using sinks::ostream_sink_mt;
        using sinks::ostream_sink_st;

        using sinks::ringbuffer_sink;
        using sinks::ringbuffer_sink_mt;
        using sinks::ringbuffer_sink_st;

        using sinks::rotating_file_sink;
        using sinks::rotating_file_sink_mt;
        using sinks::rotating_file_sink_st;

        using sinks::stderr_sink;
        using sinks::stderr_sink_mt;
        using sinks::stderr_sink_st;
        using sinks::stdout_sink;
        using sinks::stdout_sink_base;
        using sinks::stdout_sink_mt;
        using sinks::stdout_sink_st;

        using sinks::stderr_color_sink_mt;
        using sinks::stderr_color_sink_st;
        using sinks::stdout_color_sink_mt;
        using sinks::stdout_color_sink_st;

        using sinks::tcp_sink;
        using sinks::tcp_sink_config;
        using sinks::tcp_sink_mt;
        using sinks::tcp_sink_st;

        using sinks::udp_sink;
        using sinks::udp_sink_config;
        using sinks::udp_sink_mt;
        using sinks::udp_sink_st;

#ifdef _WIN32
        namespace win_eventlog
        {
            using win_eventlog::win_eventlog_sink;
        }

        using sinks::win_eventlog_sink_mt;
        using sinks::win_eventlog_sink_st;
#endif
    } // namespace sinks

#ifdef __ANDROID__
    using spdlog::android_logger_mt;
    using spdlog::android_logger_st;
#endif
    using spdlog::basic_logger_mt;
    using spdlog::basic_logger_st;
    using spdlog::callback_logger_mt;
    using spdlog::callback_logger_st;
    using spdlog::daily_logger_format_mt;
    using spdlog::daily_logger_format_st;
    using spdlog::daily_logger_mt;
    using spdlog::daily_logger_st;
    using spdlog::hourly_logger_mt;
    using spdlog::hourly_logger_st;
    using spdlog::null_logger_mt;
    using spdlog::null_logger_st;
    using spdlog::rotating_logger_mt;
    using spdlog::rotating_logger_st;
    using spdlog::stderr_color_mt;
    using spdlog::stderr_color_st;
    using spdlog::stderr_logger_mt;
    using spdlog::stderr_logger_st;
    using spdlog::stdout_color_mt;
    using spdlog::stdout_color_st;
    using spdlog::stdout_logger_mt;
    using spdlog::stdout_logger_st;
    using spdlog::udp_logger_mt;
} // namespace spdlog
