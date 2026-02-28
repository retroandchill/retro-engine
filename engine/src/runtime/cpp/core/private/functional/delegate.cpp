/**
 * @file delegate.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.functional.delegate;

namespace retro
{
    namespace
    {
        std::atomic<std::uint64_t> next_cookie{1};
    }

    std::uint64_t DelegateHandle::generate_new_cookie() noexcept
    {
        return next_cookie.fetch_add(1, std::memory_order_relaxed);
    }
} // namespace retro
