/**
 * @file scoped_service_provider.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.di;

import :scoped_service_provider;

namespace retro
{
    ScopedServiceProvider::ScopedServiceProvider(Name tag, std::shared_ptr<ServiceScope> parent_scope)
        : tag_{tag}, parent_scope_{std::move(parent_scope)}
    {
    }

    void *ScopedServiceProvider::get_raw(const std::type_info &type)
    {
        if (type == typeid(ServiceScopeFactory) || type == typeid(ServiceProvider))
        {
            return this;
        }

        if (const auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
            existing != services_.end())
        {
            return get_or_create(type, existing->second).ptr();
        }

        if (parent_scope_ != nullptr)
        {
            return parent_scope_->service_provider().get_raw(type);
        }

        return nullptr;
    }

    std::generator<void *> ScopedServiceProvider::get_all(const std::type_info &type)
    {
        if (parent_scope_ != nullptr)
        {
            co_yield std::ranges::elements_of(parent_scope_->service_provider().get_all(type));
        }

        using Pair = decltype(services_)::value_type;
        co_yield std::ranges::elements_of(
            services_ | std::views::filter([&type](const Pair &pair) { return pair.first.id.type == type; }) |
            std::views::values |
            std::views::transform([this, &type](ServiceCallSite &call_site)
                                  { return get_or_create(type, call_site).ptr(); }));
    }

    Name ScopedServiceProvider::name()
    {
        return tag_;
    }

    ServiceProvider &ScopedServiceProvider::service_provider()
    {
        return *this;
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope()
    {
        return create_scope(Name::none());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Name name)
    {
        return std::make_shared<ScopedServiceProvider>(name, shared_from_this());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Delegate<void(ServiceCollection &)> configure)
    {
        // TODO: Run configuration
        return create_scope();
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Name name,
                                                                      Delegate<void(ServiceCollection &)> configure)
    {
        // TODO: Run configuration
        return create_scope(name);
    }

    const ServiceInstance &ScopedServiceProvider::get_or_create(std::type_index type, ServiceCallSite &call_site)
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
