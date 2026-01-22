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
        auto existing_singleton = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
        if (existing_singleton != services_.end())
        {
            return get_or_create(existing_singleton->second);
        }

        throw ServiceNotFoundException{};
    }

    std::shared_ptr<void> ServiceProvider::get_or_create(ServiceCallSite &call_site)
    {
        return std::visit(Overload{[](const RealizedSingleton &singleton) { return singleton.ptr; },
                                   [&](const UnrealizedService &service)
                                   {
                                       auto created = service.registration(*this);
                                       if (service.is_singleton)
                                       {
                                           call_site.emplace<RealizedSingleton>(created);
                                       }
                                       return created;
                                   }},
                          call_site);
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type,
                                             ServiceFactory factory,
                                             const bool is_singleton) noexcept
        : type(type), registration(std::in_place_type<UnrealizedService>, std::move(factory), is_singleton)
    {
    }

    ServiceRegistration::ServiceRegistration(const std::type_info &type, std::shared_ptr<void> ptr) noexcept
        : type(type), registration(std::in_place_type<RealizedSingleton>, std::move(ptr))
    {
    }

    void ServiceCollection::add_singleton(const std::type_info &type, Factory factory)
    {
        registrations_.emplace_back(type, std::move(factory), true);
    }

    void ServiceCollection::add_singleton(const std::type_info &type, std::shared_ptr<void> ptr)
    {
        registrations_.emplace_back(type, std::move(ptr));
    }

    void ServiceCollection::add_transient(const std::type_info &type, Factory factory)
    {
        registrations_.emplace_back(type, std::move(factory), false);
    }

    ServiceProvider create_service_provider(ServiceCollection &services)
    {
        return ServiceProvider{services};
    }
} // namespace retro
