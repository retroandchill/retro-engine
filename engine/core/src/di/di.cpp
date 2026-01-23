/**
 * @file di.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core;

namespace retro
{
    const char *ServiceNotFoundException::what() const noexcept
    {
        return "The requested service was not found";
    }

    ServiceProvider::ServiceProvider(ServiceCollection &service_collection)
    {
        std::unordered_map<ServiceIdentifier, uint32> service_count;
        for (auto &registration : service_collection.registrations_)
        {
            ServiceIdentifier id{registration.type};
            ServiceCacheKey key{.id = id, .slot = service_count[id]++};
            services_.emplace(key, registration.registration);
        }
    }

    void *ServiceProvider::get_raw(const std::type_info &type)
    {
        return get_shared_impl(type).get();
    }

    std::shared_ptr<void> ServiceProvider::get_shared_impl(const std::type_info &type)
    {
        auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
        if (existing != services_.end())
        {
            return get_or_create(existing->second);
        }

        throw ServiceNotFoundException{};
    }

    std::shared_ptr<void> ServiceProvider::get_or_create(ServiceCallSite &call_site)
    {
        return std::visit(Overload{[](const RealizedSingleton &singleton) { return singleton.ptr; },
                                   [&](const UnrealizedSingleton service)
                                   {
                                       auto created = service.registration(*this);
                                       call_site.emplace<RealizedSingleton>(created);
                                       return created;
                                   },
                                   [](DerivedTransient) -> std::shared_ptr<void> { throw ServiceNotFoundException{}; },
                                   [](const DirectTransient) -> std::shared_ptr<void>
                                   {
                                       throw ServiceNotFoundException{};
                                   }},
                          call_site);
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type) noexcept
        : type{type}, registration{std::in_place_type<DirectTransient>}
    {
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type, SingletonCreator factory) noexcept
        : type{type}, registration{std::in_place_type<UnrealizedSingleton>, factory}
    {
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type, TransientCreator factory) noexcept
        : type{type}, registration{std::in_place_type<DerivedTransient>, factory}
    {
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type, std::shared_ptr<void> ptr) noexcept
        : type(type), registration(std::in_place_type<RealizedSingleton>, std::move(ptr))
    {
    }

    ServiceProvider create_service_provider(ServiceCollection &services)
    {
        return ServiceProvider{services};
    }
} // namespace retro
