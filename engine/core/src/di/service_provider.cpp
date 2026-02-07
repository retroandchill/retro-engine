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

    ServiceProviderImpl::ServiceProviderImpl(const std::span<const ServiceRegistration> registrations)
    {
        std::unordered_map<ServiceIdentifier, std::uint32_t> service_count;
        for (auto &registration : registrations)
        {
            ServiceIdentifier id{registration.type};
            ServiceCacheKey key{.id = id, .slot = service_count[id]++};
            services_.emplace(key, registration.registration);
        }
    }

    ServiceProviderImpl::~ServiceProviderImpl() noexcept
    {
        for (auto &singleton : created_services_ | std::views::reverse)
        {
            singleton.dispose();
        }
    }

    void *ServiceProviderImpl::get_raw(const std::type_info &type)
    {
        if (const auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
            existing != services_.end())
        {
            return get_or_create(type, existing->second).ptr();
        }

        return nullptr;
    }

    std::generator<void *> ServiceProviderImpl::get_all(const std::type_info &type)
    {
        using Pair = decltype(services_)::value_type;
        co_yield std::ranges::elements_of(
            services_ | std::views::filter([&type](const Pair &pair) { return pair.first.id.type == type; }) |
            std::views::values |
            std::views::transform([this, &type](ServiceCallSite &call_site)
                                  { return get_or_create(type, call_site).ptr(); }));
    }

    const ServiceInstance &ServiceProviderImpl::get_or_create(std::type_index type, ServiceCallSite &call_site)
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
                                       return created;
                                   }},
                          call_site);
    }
} // namespace retro
