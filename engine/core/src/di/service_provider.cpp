/**
 * @file service_provider.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.di;

namespace retro
{
    const char *ServiceNotFoundException::what() const noexcept
    {
        return "The requested service was not found";
    }

    ServiceProvider::ServiceProvider(ServiceCollection &service_collection)
    {
        std::unordered_map<ServiceIdentifier, std::uint32_t> service_count;
        for (auto &registration : service_collection.registrations_)
        {
            ServiceIdentifier id{registration.type};
            ServiceCacheKey key{.id = id, .slot = service_count[id]++};
            services_.emplace(key, registration.registration);
        }
    }

    ServiceProvider::~ServiceProvider() noexcept
    {
        for (auto &singleton : created_services_ | std::views::reverse)
        {
            singleton.dispose();
        }
    }

    void *ServiceProvider::get_raw(const std::type_info &type)
    {
        auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
        if (existing != services_.end())
        {
            return get_or_create(existing->second).ptr();
        }

        throw ServiceNotFoundException{};
    }

    const ServiceInstance &ServiceProvider::get_or_create(ServiceCallSite &call_site)
    {
        return std::visit(Overload{[&](const RealizedService &singleton) -> auto &
                                   { return created_services_[singleton.instance_index]; },
                                   [&](const UnrealizedService &service) -> auto &
                                   {
                                       auto &created =
                                           created_services_.emplace_back(service.registration.execute(*this));
                                       if (service.lifetime != ServiceLifetime::Transient)
                                       {
                                           call_site.emplace<RealizedService>(created_services_.size() - 1);
                                       }
                                       service.configure.broadcast(created.ptr(), *this);
                                       return created;
                                   }},
                          call_site);
    }
} // namespace retro
