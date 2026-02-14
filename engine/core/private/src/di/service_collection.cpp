/**
 * @file service_collection.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.di;

import :service_collection;
import :scoped_service_provider;

namespace retro
{
    std::shared_ptr<ServiceProvider> ServiceCollection::create_service_provider() const
    {
        return std::make_shared<ScopedServiceProvider>(registrations_);
    }
} // namespace retro
