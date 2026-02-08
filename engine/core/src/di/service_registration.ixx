/**
 * @file service_registration.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_registration;

import std;
import :service_call_site;

namespace retro
{
    struct ServiceRegistration
    {
        std::type_index type;
        std::shared_ptr<UnrealizedService> registration;

        inline ServiceRegistration(const std::type_info &type,
                                   ServiceLifetime lifetime,
                                   ServiceFactory factory) noexcept
            : type{type}, registration{std::make_shared<UnrealizedService>(lifetime, std::move(factory))}
        {
        }

        template <typename T>
        ServiceRegistration(const std::type_info &type, std::shared_ptr<T> ptr) noexcept
            : type{type},
              registration{std::make_shared<UnrealizedService>(ServiceLifetime::Singleton,
                                                               [p = std::move(ptr)](auto &)
                                                               { return ServiceInstance::from_shared(std::move(p)); })}
        {
        }

        template <typename T>
        ServiceRegistration(const std::type_info &type, std::unique_ptr<T> ptr) noexcept
            : type{type},
              registration{std::make_shared<UnrealizedService>(ServiceLifetime::Singleton,
                                                               [p = std::move(ptr)](auto &)
                                                               { return ServiceInstance::from_unique(std::move(p)); })}
        {
        }

        template <typename T>
        ServiceRegistration(const std::type_info &type, RefCountPtr<T> ptr) noexcept
            : type{type}, registration{std::make_shared<UnrealizedService>(
                              ServiceLifetime::Singleton,
                              [p = std::move(ptr)](auto &) { return ServiceInstance::from_intrusive(std::move(p)); })}
        {
        }
    };
} // namespace retro
