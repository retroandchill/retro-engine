/**
 * @file service_call_site.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_call_site;

import std;
import :service_instance;
import retro.core.strings.name;

namespace retro
{
    export struct SingletonScope
    {
    };

    export struct ScopedScope
    {
        Name tag;
    };

    export struct TransientScope
    {
    };

    export using ServiceLifetime = std::variant<SingletonScope, ScopedScope, TransientScope>;

    export constexpr ServiceLifetime singleton_service_lifetime = SingletonScope{};

    export constexpr ServiceLifetime scoped_service_lifetime(const Name tag = Name::none()) noexcept
    {
        return ScopedScope{tag};
    }

    export constexpr ServiceLifetime transient_service_lifetime = TransientScope{};

    export enum class ScopeLevel : std::uint8_t
    {
        Root = 0,
        Nested = 1
    };

    using ServiceFactory = Delegate<ServiceInstance(class ServiceProvider &)>;

    struct RealizedService
    {
        std::size_t instance_index = static_cast<std::size_t>(-1);
    };

    struct UnrealizedService
    {
        ServiceLifetime lifetime;
        Name scope_tag = Name::none();
        ServiceFactory registration;

        UnrealizedService(const ServiceLifetime lifetime, ServiceFactory registration) noexcept
            : lifetime{lifetime}, registration{std::move(registration)}
        {
        }

        UnrealizedService(const ServiceLifetime lifetime, Name scope_tag, ServiceFactory factory) noexcept
            : lifetime{lifetime}, scope_tag{std::move(scope_tag)}, registration{std::move(factory)}
        {
        }
    };

    using ServiceCallSite = std::variant<std::shared_ptr<UnrealizedService>, RealizedService>;
} // namespace retro
