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

namespace boost
{
    export using boost::noncopyable;

    export using boost::simple_segregated_storage;
    export using boost::default_user_allocator_new_delete;
    export using boost::default_user_allocator_malloc_free;
    export using boost::pool;
    export using boost::object_pool;
    export using boost::singleton_pool;
    export using boost::pool_allocator_tag;
    export using boost::pool_allocator;
    export using boost::fast_pool_allocator_tag;
    export using boost::fast_pool_allocator;

    export using boost::unordered_flat_map;

    namespace dll
    {
        export using dll::shared_library;
        export using dll::import_symbol;
        export using dll::library_info;
        export using dll::symbol_location_ptr;
        export using dll::symbol_location;
        export using dll::program_location;
        export using dll::operator==;
        export using dll::operator!=;
        export using dll::swap;
    } // namespace dll

    namespace uuids
    {
        export using uuids::uuid;
        export using uuids::operator==;
        export using uuids::operator!=;
        export using uuids::operator<;
        export using uuids::operator<=;
        export using uuids::operator>;
        export using uuids::operator>=;
        export using uuids::operator<=>;

        export using uuids::swap;
        export using uuids::hash_value;

        export using uuids::to_chars;
        export using uuids::operator<<;
        export using uuids::operator>>;
        export using uuids::to_string;
        export using uuids::to_wstring;

        export using uuids::nil_generator;
        export using uuids::string_generator;
        export using uuids::name_generator;
        export using uuids::random_generator;
        export using uuids::time_generator_v1;
        export using uuids::time_generator_v6;
        export using uuids::time_generator_v7;

        namespace ns
        {
            export using ns::dns;
            export using ns::url;
            export using ns::oid;
            export using ns::x500dn;
        } // namespace ns

        export using uuids::nil_uuid;
    } // namespace uuids

    export using boost::none_t;
    export using boost::none;
    export using boost::in_place_init_t;
    export using boost::in_place_init;
    export using boost::in_place_init_if_t;
    export using boost::in_place_init_if;
    export using boost::optional;
    export using boost::swap;
    export using boost::make_optional;
    export using boost::get;
    export using boost::get_optional_value_or;
    export using boost::get_pointer;
    export using boost::operator<<;
    export using boost::operator==;
    export using boost::operator<;
    export using boost::operator>;
    export using boost::operator<=;
    export using boost::operator>=;

} // namespace boost
