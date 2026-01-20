/**
 * @file simple_allocator.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:memory.simple_allocator;

import std;

namespace retro
{
    /**
     * Public export of the concept for the minimum requirements for a type to be a valid allocator
     * as published in the C++26 standard.
     *
     * @tparam Alloc The allocator type under test.
     */
    export template <class Alloc>
    concept SimpleAllocator = requires(Alloc alloc, std::size_t n) {
        // ReSharper disable once CppRedundantTypenameKeyword
        {
            *alloc.allocate(n)
        } -> std::same_as<typename Alloc::value_type &>;
        {
            alloc.deallocate(alloc.allocate(n), n)
        };
    } && std::copy_constructible<Alloc> && std::equality_comparable<Alloc>;
} // namespace retro
