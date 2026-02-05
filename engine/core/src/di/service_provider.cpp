/**
 * @file di.cpp
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
        for (auto &singleton : singletons_ | std::views::reverse)
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

    std::shared_ptr<void> ServiceProvider::get_shared_impl(const std::type_info &type)
    {
        auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
        if (existing != services_.end())
        {
            auto &created = get_or_create(existing->second);
            if (!created.has_shared_storage())
            {
                throw ServiceNotFoundException{};
            }

            return created.shared_ptr();
        }

        throw ServiceNotFoundException{};
    }

    const ServiceInstance &ServiceProvider::get_or_create(ServiceCallSite &call_site)
    {
        return std::visit(Overload{[&](const RealizedSingleton &singleton) -> auto &
                                   { return singletons_[singleton.instance_index]; },
                                   [&](const UnrealizedSingleton &service) -> auto &
                                   {
                                       auto &created = singletons_.emplace_back(service.registration.execute(*this));
                                       call_site.emplace<RealizedSingleton>(singletons_.size() - 1);
                                       return created;
                                   },
                                   [](const DerivedTransient &) -> ServiceInstance &
                                   { throw ServiceNotFoundException{}; },
                                   [](const DirectTransient) -> ServiceInstance &
                                   {
                                       throw ServiceNotFoundException{};
                                   }},
                          call_site);
    }
} // namespace retro
