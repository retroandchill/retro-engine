/**
 * @file service_collection.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.di;

namespace retro
{
    ServiceRegistration::ServiceRegistration(const std::type_info &type,
                                             ServiceLifetime lifetime,
                                             ServiceFactory factory) noexcept
        : type{type}, registration{std::in_place_type<UnrealizedService>, lifetime, factory}
    {
    }

    ServiceProvider create_service_provider(ServiceCollection &services)
    {
        return ServiceProvider{services};
    }
} // namespace retro
