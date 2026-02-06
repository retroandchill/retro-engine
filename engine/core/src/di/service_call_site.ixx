/**
 * @file service_call_site.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_call_site;

import std;
import :service_instance;

namespace retro
{
    export enum class ServiceLifetime : std::uint8_t
    {
        Singleton,
        Transient
    };

    using ServiceFactory = Delegate<ServiceInstance(class ServiceProvider &)>;

    using ConfigureService = MulticastDelegate<void(void *, ServiceProvider &)>;

    struct RealizedService
    {
        std::size_t instance_index = static_cast<std::size_t>(-1);
    };

    struct UnrealizedService
    {
        ServiceLifetime lifetime{};
        ServiceFactory registration{};
    };

    using ServiceCallSite = std::variant<UnrealizedService, RealizedService>;
} // namespace retro
