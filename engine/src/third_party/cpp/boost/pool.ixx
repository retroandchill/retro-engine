/**
 * @file pool.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/pool/pool_alloc.hpp>

export module boost.pool;

export namespace boost
{
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

} // namespace boost
