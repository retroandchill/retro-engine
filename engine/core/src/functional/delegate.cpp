/**
 * @file delegate.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.functional.delegate;

namespace retro
{
    std::atomic<std::uint64_t> DelegateHandle::next_cookie_{1};
}
