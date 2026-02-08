/**
 * @file service_provider.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.di;

import :service_provider;

namespace retro
{
    const char *ServiceNotFoundException::what() const noexcept
    {
        return "The requested service was not found";
    }
} // namespace retro
