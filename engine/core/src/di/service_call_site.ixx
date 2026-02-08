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
    export enum class ServiceLifetime : std::uint8_t
    {
        Singleton,
        Scoped,
        Transient
    };

    export enum class ScopeLevel : std::uint8_t
    {
        Root = 0,
        Nested = 1
    };

    export struct ScopingRules
    {
        bool allow_singleton = false;
        bool allow_transient = false;
        bool allow_scoped = false;

        [[nodiscard]] static constexpr ScopingRules root_scope()
        {
            return {
                .allow_singleton = true,
                .allow_transient = true,
                .allow_scoped = false,
            };
        }

        [[nodiscard]] static constexpr ScopingRules nested_scope()
        {
            return {
                .allow_singleton = false,
                .allow_transient = true,
                .allow_scoped = true,
            };
        }

        [[nodiscard]] constexpr bool can_resolve(const ServiceLifetime lifetime) const noexcept
        {
            switch (lifetime)
            {
                case ServiceLifetime::Singleton:
                    return allow_singleton;
                case ServiceLifetime::Scoped:
                    return allow_scoped;
                case ServiceLifetime::Transient:
                    return allow_transient;
            }

            return false;
        }
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
