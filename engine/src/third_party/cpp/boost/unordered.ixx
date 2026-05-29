/**
 * @file unordered.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

// ReSharper disable CppUnusedIncludeDirective
#include <boost/unordered/concurrent_flat_map.hpp>
#include <boost/unordered/concurrent_flat_set.hpp>
#include <boost/unordered/concurrent_node_map.hpp>
#include <boost/unordered/concurrent_node_set.hpp>
#include <boost/unordered/unordered_flat_map.hpp>
#include <boost/unordered/unordered_flat_set.hpp>
#include <boost/unordered/unordered_map.hpp>
#include <boost/unordered/unordered_node_map.hpp>
#include <boost/unordered/unordered_node_set.hpp>
#include <boost/unordered/unordered_set.hpp>
// ReSharper restore CppUnusedIncludeDirective

export module boost.unordered;

export namespace boost
{
    using boost::unordered_map;
    using boost::unordered_multimap;
    using boost::unordered_multiset;
    using boost::unordered_set;

    using boost::unordered_flat_map;
    using boost::unordered_flat_set;
    using boost::unordered_node_map;
    using boost::unordered_node_set;

    using boost::concurrent_flat_map;
    using boost::concurrent_flat_set;
    using boost::concurrent_node_map;
    using boost::concurrent_node_set;
} // namespace boost
