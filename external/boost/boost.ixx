/**
 * @file boost.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define BOOST_DLL_USE_STD_FS
#define BOOST_DLL_USE_BOOST_SHARED_PTR
#include "intrusive.hpp"

#include <boost/asio.hpp>
#include <boost/di.hpp>
#include <boost/dll.hpp>
#include <boost/optional.hpp>
#include <boost/pool/pool_alloc.hpp>
#include <boost/unordered/unordered_flat_map.hpp>
#include <boost/uuid.hpp>

export module boost;

export namespace boost
{
#pragma region core
    using boost::noncopyable;

    namespace system
    {
        using system::error_code;
        using system::system_error;

        namespace errc
        {
            using errc::address_family_not_supported;
            using errc::address_in_use;
            using errc::address_not_available;
            using errc::already_connected;
            using errc::argument_list_too_long;
            using errc::argument_out_of_domain;
            using errc::bad_address;
            using errc::bad_file_descriptor;
            using errc::bad_message;
            using errc::broken_pipe;
            using errc::connection_aborted;
            using errc::connection_already_in_progress;
            using errc::connection_refused;
            using errc::connection_reset;
            using errc::cross_device_link;
            using errc::destination_address_required;
            using errc::device_or_resource_busy;
            using errc::directory_not_empty;
            using errc::errc_t;
            using errc::executable_format_error;
            using errc::file_exists;
            using errc::file_too_large;
            using errc::filename_too_long;
            using errc::function_not_supported;
            using errc::host_unreachable;
            using errc::identifier_removed;
            using errc::illegal_byte_sequence;
            using errc::inappropriate_io_control_operation;
            using errc::interrupted;
            using errc::invalid_argument;
            using errc::invalid_seek;
            using errc::io_error;
            using errc::is_a_directory;
            using errc::make_error_code;
            using errc::message_size;
            using errc::network_down;
            using errc::network_reset;
            using errc::network_unreachable;
            using errc::no_buffer_space;
            using errc::no_child_process;
            using errc::no_link;
            using errc::no_lock_available;
            using errc::no_message;
            using errc::no_message_available;
            using errc::no_protocol_option;
            using errc::no_space_on_device;
            using errc::no_stream_resources;
            using errc::no_such_device;
            using errc::no_such_device_or_address;
            using errc::no_such_file_or_directory;
            using errc::no_such_process;
            using errc::not_a_directory;
            using errc::not_a_socket;
            using errc::not_a_stream;
            using errc::not_connected;
            using errc::not_enough_memory;
            using errc::not_supported;
            using errc::operation_canceled;
            using errc::operation_in_progress;
            using errc::operation_not_permitted;
            using errc::operation_not_supported;
            using errc::operation_would_block;
            using errc::owner_dead;
            using errc::permission_denied;
            using errc::protocol_error;
            using errc::protocol_not_supported;
            using errc::read_only_file_system;
            using errc::resource_deadlock_would_occur;
            using errc::resource_unavailable_try_again;
            using errc::result_out_of_range;
            using errc::state_not_recoverable;
            using errc::stream_timeout;
            using errc::success;
            using errc::text_file_busy;
            using errc::timed_out;
            using errc::too_many_files_open;
            using errc::too_many_files_open_in_system;
            using errc::too_many_links;
            using errc::too_many_symbolic_link_levels;
            using errc::value_too_large;
            using errc::wrong_protocol_type;
        } // namespace errc
    }     // namespace system
#pragma endregion

#pragma region pool
    using boost::default_user_allocator_malloc_free;
    using boost::default_user_allocator_new_delete;
    using boost::fast_pool_allocator;
    using boost::fast_pool_allocator_tag;
    using boost::object_pool;
    using boost::pool;
    using boost::pool_allocator;
    using boost::pool_allocator_tag;
    using boost::simple_segregated_storage;
    using boost::singleton_pool;
#pragma endregion

#pragma region unordered
    using boost::unordered_flat_map;
#pragma endregion

#pragma region dll
    namespace dll
    {
        using dll::import_symbol;
        using dll::library_info;
        using dll::program_location;
        using dll::shared_library;
        using dll::symbol_location;
        using dll::symbol_location_ptr;
        using dll::operator==;
        using dll::operator!=;
        using dll::swap;
    } // namespace dll
#pragma endregion

#pragma region uuid
    namespace uuids
    {
        using uuids::uuid;
        using uuids::operator==;
        using uuids::operator!=;
        using uuids::operator<;
        using uuids::operator<=;
        using uuids::operator>;
        using uuids::operator>=;
        using uuids::operator<=>;

        using uuids::hash_value;
        using uuids::swap;

        using uuids::to_chars;
        using uuids::operator<<;
        using uuids::operator>>;
        using uuids::to_string;
        using uuids::to_wstring;

        using uuids::name_generator;
        using uuids::nil_generator;
        using uuids::random_generator;
        using uuids::string_generator;
        using uuids::time_generator_v1;
        using uuids::time_generator_v6;
        using uuids::time_generator_v7;

        namespace ns
        {
            using ns::dns;
            using ns::oid;
            using ns::url;
            using ns::x500dn;
        } // namespace ns

        using uuids::nil_uuid;
    } // namespace uuids
#pragma endregion

#pragma region optional
    using boost::get;
    using boost::get_optional_value_or;
    using boost::get_pointer;
    using boost::in_place_init;
    using boost::in_place_init_if;
    using boost::in_place_init_if_t;
    using boost::in_place_init_t;
    using boost::make_optional;
    using boost::none;
    using boost::none_t;
    using boost::optional;
    using boost::swap;
    using boost::operator<<;
    using boost::operator==;
    using boost::operator<;
    using boost::operator>;
    using boost::operator<=;
    using boost::operator>=;
#pragma endregion

#pragma region intrusive
    namespace intrusive
    {
        using intrusive::list;
        using intrusive::list_base_hook;
        using intrusive::list_member_hook;
        using intrusive::slist;
        using intrusive::slist_base_hook;
        using intrusive::slist_member_hook;

        using intrusive::bs_multiset;
        using intrusive::bs_set;
        using intrusive::bs_set_base_hook;
        using intrusive::bs_set_member_hook;
        using intrusive::bstree;

        using intrusive::multiset;
        using intrusive::rbtree;
        using intrusive::set;
        using intrusive::set_base_hook;
        using intrusive::set_member_hook;

        using intrusive::avl_multiset;
        using intrusive::avl_set;
        using intrusive::avl_set_base_hook;
        using intrusive::avl_set_member_hook;
        using intrusive::avltree;

        using intrusive::splay_multiset;
        using intrusive::splay_set;
        using intrusive::splaytree;

        using intrusive::sg_multiset;
        using intrusive::sg_set;
        using intrusive::sgtree;

        using intrusive::treap;
        using intrusive::treap_multiset;
        using intrusive::treap_set;

        using intrusive::hashtable;
        using intrusive::unordered_multiset;
        using intrusive::unordered_set;
        using intrusive::unordered_set_base_hook;
        using intrusive::unordered_set_member_hook;

        using intrusive::any_base_hook;
        using intrusive::any_member_hook;

        using intrusive::base_hook;
        using intrusive::bucket_traits;
        using intrusive::cache_begin;
        using intrusive::cache_last;
        using intrusive::compare;
        using intrusive::compare_hash;
        using intrusive::constant_time_size;
        using intrusive::equal;
        using intrusive::floating_point;
        using intrusive::function_hook;
        using intrusive::hash;
        using intrusive::incremental;
        using intrusive::linear;
        using intrusive::link_mode;
        using intrusive::member_hook;
        using intrusive::optimize_multikey;
        using intrusive::optimize_size;
        using intrusive::power_2_buckets;
        using intrusive::priority;
        using intrusive::size_type;
        using intrusive::store_hash;
        using intrusive::tag;
        using intrusive::value_traits;
        using intrusive::void_pointer;

        using intrusive::derivation_value_traits;
        using intrusive::trivial_value_traits;
        using intrusive::value_traits;

        using intrusive::key_of_value;
        using intrusive::pointer_plus_bits;

        using intrusive::auto_unlink;
        using intrusive::normal_link;
        using intrusive::safe_link;

        namespace detail
        {
            using detail::destructor_impl;
        }
    } // namespace intrusive
#pragma endregion

#pragma region di
    namespace ext::di
    {
        using di::bind;
        using di::create;
        using di::injector;
        using di::make_injector;
        using di::named;
        using di::override;

        using di::deduce;
        using di::singleton;
        using di::unique;
    } // namespace ext::di

    // We need this so IntelliSense properly picks up the alias
    namespace di = di;
#pragma endregion

#pragma region asio

    namespace asio
    {
        using asio::any_io_executor;
        using asio::basic_stream_file;
        using asio::execution_context;
        using asio::stream_file;
        using asio::system_executor;

        using asio::file_base;

        namespace error
        {
            using error::bad_descriptor;
            using error::make_error_code;
        } // namespace error
    }     // namespace asio

#pragma endregion
} // namespace boost
