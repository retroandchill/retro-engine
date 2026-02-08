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
    ScopedServiceProvider::ScopedServiceProvider(const std::span<const ServiceRegistration> registrations)
        : ScopedServiceProvider{registrations, Name::none(), nullptr, ScopingRules::root_scope()}
    {
    }

    ScopedServiceProvider::ScopedServiceProvider(const std::span<const ServiceRegistration> registrations,
                                                 const Name tag,
                                                 std::shared_ptr<ServiceScope> parent_scope,
                                                 const ScopingRules rules)
        : tag_{tag}, parent_scope_{std::move(parent_scope)}, rules_{rules},
          scope_level_{parent_scope_ != nullptr ? parent_scope_->scope_level() + 1 : 0},
          service_registrations_{std::from_range, registrations}
    {
        std::unordered_map<ServiceIdentifier, std::uint32_t> service_count;
        for (auto &registration : service_registrations_)
        {
            const auto &call_site = registration.registration;
            if (!rules_.can_resolve(call_site->lifetime) ||
                (!call_site->scope_tag.is_none() && call_site->scope_tag != tag_))
            {
                continue;
            }

            ServiceIdentifier id{registration.type};
            ServiceCacheKey key{.id = id, .slot = service_count[id]++};
            services_.emplace(key, call_site);
        }
    }

    ScopedServiceProvider::~ScopedServiceProvider() noexcept
    {
        for (auto &singleton : created_services_ | std::views::reverse)
        {
            singleton.dispose();
        }
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
            return get_or_create(existing->second).ptr();
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
            std::views::transform([this](ServiceCallSite &call_site) { return get_or_create(call_site).ptr(); }));
    }

    Name ScopedServiceProvider::name()
    {
        return tag_;
    }

    ServiceProvider &ScopedServiceProvider::service_provider()
    {
        return *this;
    }

    const ScopingRules &ScopedServiceProvider::scoping_rules() const
    {
        return rules_;
    }

    std::uint32_t ScopedServiceProvider::scope_level() const
    {
        return scope_level_;
    }

    bool ScopedServiceProvider::is_root_scope() const
    {
        return parent_scope_ == nullptr;
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::parent_scope() const
    {
        return parent_scope_;
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope()
    {
        return create_scope(Name::none(), ScopingRules::nested_scope());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Name name)
    {
        return create_scope(name, ScopingRules::nested_scope());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        const Delegate<void(ServiceCollection &)> &configure)
    {
        return create_scope(ScopingRules::nested_scope(), configure);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        Name name,
        const Delegate<void(ServiceCollection &)> &configure)
    {
        return create_scope(name, ScopingRules::nested_scope(), configure);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(const ScopingRules &rules)
    {
        return create_scope(Name::none(), rules);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Name name, const ScopingRules &rules)
    {
        return std::make_shared<ScopedServiceProvider>(service_registrations_, name, shared_from_this(), rules);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        const ScopingRules &rules,
        const Delegate<void(ServiceCollection &)> &configure)
    {
        return create_scope(Name::none(), rules, configure);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        const Name name,
        const ScopingRules &rules,
        const Delegate<void(ServiceCollection &)> &configure)
    {
        ServiceCollection services{service_registrations_};
        configure.execute(services);
        return create_scope(name, rules);
    }

    const ServiceInstance &ScopedServiceProvider::get_or_create(ServiceCallSite &call_site)
    {
        return std::visit(Overload{[&](const RealizedService &singleton) -> auto &
                                   { return created_services_[singleton.instance_index]; },
                                   [&](const std::shared_ptr<UnrealizedService> &service) -> auto &
                                   {
                                       auto &created =
                                           created_services_.emplace_back(service->registration.execute(*this));
                                       if (service->lifetime != ServiceLifetime::Transient)
                                       {
                                           call_site.emplace<RealizedService>(created_services_.size() - 1);
                                       }
                                       return created;
                                   }},
                          call_site);
    }
} // namespace retro
