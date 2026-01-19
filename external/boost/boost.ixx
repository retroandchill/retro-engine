/**
 * @file boost.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define BOOST_DLL_USE_STD_FS
#define BOOST_DLL_USE_BOOST_SHARED_PTR
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

} // namespace boost
