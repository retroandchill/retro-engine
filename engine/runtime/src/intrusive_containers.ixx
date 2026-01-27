/**
 * @file intrusive_containers.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/intrusive/any_hook.hpp>
#include <boost/intrusive/avl_set.hpp>
#include <boost/intrusive/avl_set_hook.hpp>
#include <boost/intrusive/avltree.hpp>
#include <boost/intrusive/avltree_algorithms.hpp>
#include <boost/intrusive/bs_set.hpp>
#include <boost/intrusive/bs_set_hook.hpp>
#include <boost/intrusive/bstree.hpp>
#include <boost/intrusive/bstree_algorithms.hpp>
#include <boost/intrusive/circular_list_algorithms.hpp>
#include <boost/intrusive/circular_slist_algorithms.hpp>
#include <boost/intrusive/derivation_value_traits.hpp>
#include <boost/intrusive/hashtable.hpp>
#include <boost/intrusive/intrusive_fwd.hpp>
#include <boost/intrusive/linear_slist_algorithms.hpp>
#include <boost/intrusive/link_mode.hpp>
#include <boost/intrusive/list.hpp>
#include <boost/intrusive/list_hook.hpp>
#include <boost/intrusive/member_value_traits.hpp>
#include <boost/intrusive/options.hpp>
#include <boost/intrusive/pack_options.hpp>
#include <boost/intrusive/parent_from_member.hpp>
#include <boost/intrusive/pointer_plus_bits.hpp>
#include <boost/intrusive/pointer_rebind.hpp>
#include <boost/intrusive/pointer_traits.hpp>
#include <boost/intrusive/priority_compare.hpp>
#include <boost/intrusive/rbtree.hpp>
#include <boost/intrusive/rbtree_algorithms.hpp>
#include <boost/intrusive/set.hpp>
#include <boost/intrusive/set_hook.hpp>
#include <boost/intrusive/sg_set.hpp>
#include <boost/intrusive/sgtree.hpp>
#include <boost/intrusive/sgtree_algorithms.hpp>
#include <boost/intrusive/slist.hpp>
#include <boost/intrusive/slist_hook.hpp>
#include <boost/intrusive/splay_set.hpp>
#include <boost/intrusive/splaytree.hpp>
#include <boost/intrusive/splaytree_algorithms.hpp>
#include <boost/intrusive/treap.hpp>
#include <boost/intrusive/treap_algorithms.hpp>
#include <boost/intrusive/treap_set.hpp>
#include <boost/intrusive/trivial_value_traits.hpp>
#include <boost/intrusive/unordered_set.hpp>
#include <boost/intrusive/unordered_set_hook.hpp>

export module retro.runtime:intrusive_containers;

// TODO: Prune this to see if we can get it down, or move it to be exported from the core
export namespace boost::intrusive
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
} // namespace boost::intrusive
