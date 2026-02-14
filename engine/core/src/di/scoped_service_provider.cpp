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
    ScopedServiceProvider::ScopedServiceProvider(const std::span<const ServiceCallSite> registrations)
        : ScopedServiceProvider{registrations, Name::none(), nullptr}
    {
    }

    ScopedServiceProvider::ScopedServiceProvider(const std::span<const ServiceCallSite> registrations,
                                                 const Name tag,
                                                 std::shared_ptr<ServiceScope> parent_scope)
        : tag_{tag}, parent_scope_{std::move(parent_scope)},
          scope_level_{parent_scope_ != nullptr ? parent_scope_->scope_level() + 1 : 0},
          service_registrations_{std::from_range, registrations}
    {
        std::unordered_map<ServiceIdentifier, std::uint32_t> service_count;
        for (auto [i, call_site] : service_registrations_ | std::views::enumerate)
        {
            if (!can_resolve(call_site))
            {
                continue;
            }

            const auto id = std::visit(Overload{[&](const InstanceServiceCallSite &instance)
                                                { return ServiceIdentifier{instance.instance().type()}; },
                                                [&](const FactoryServiceCallSite &factory)
                                                {
                                                    return ServiceIdentifier{factory.service_type()};
                                                }},
                                       call_site);

            ServiceCacheKey key{.id = id, .slot = service_count[id]++};
            registration_indices_.emplace(key, static_cast<std::size_t>(i));
        }
    }

    ScopedServiceProvider::~ScopedServiceProvider() noexcept
    {
        for (auto &service_instance : created_services_ | std::views::reverse)
        {
            service_instance.reset();
        }
    }

    void *ScopedServiceProvider::get_raw(const std::type_info &type)
    {
        if (type == typeid(ServiceScopeFactory))
        {
            return static_cast<ServiceScopeFactory *>(this);
        }

        if (type == typeid(ServiceProvider))
        {
            return static_cast<ServiceProvider *>(this);
        }

        if (const auto existing = registration_indices_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
            existing != registration_indices_.end())
        {
            return get_or_create(existing->first, service_registrations_[existing->second]).ptr();
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

        using Pair = decltype(registration_indices_)::value_type;
        co_yield std::ranges::elements_of(
            registration_indices_ |
            std::views::filter([&type](const Pair &pair) { return pair.first.id.type == type; }) |
            std::views::transform(
                [this](const Pair &pair)
                {
                    const auto &call_site = service_registrations_[pair.second];
                    return get_or_create(pair.first, call_site).ptr();
                }));
    }

    Name ScopedServiceProvider::name()
    {
        return tag_;
    }

    ServiceProvider &ScopedServiceProvider::service_provider()
    {
        return *this;
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
        return create_scope(Name::none());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(Name name)
    {
        return std::make_shared<ScopedServiceProvider>(service_registrations_, name, shared_from_this());
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        const Delegate<void(ServiceCollection &)> &configure)
    {
        return create_scope(Name::none(), configure);
    }

    std::shared_ptr<ServiceScope> ScopedServiceProvider::create_scope(
        const Name name,
        const Delegate<void(ServiceCollection &)> &configure)
    {
        ServiceCollection services{service_registrations_, scope_level_ + 1};
        configure.execute(services);
        return std::make_shared<ScopedServiceProvider>(services.registrations(), name, shared_from_this());
    }

    const ServiceInstance &ScopedServiceProvider::get_or_create(const ServiceCacheKey &key,
                                                                const ServiceCallSite &call_site)
    {
        return std::visit(
            Overload{[&](const InstanceServiceCallSite &instance) -> const ServiceInstance &
                     { return instance.instance(); },
                     [&](const FactoryServiceCallSite &factory) -> const ServiceInstance &
                     {
                         if (!std::holds_alternative<TransientScope>(factory.lifetime()))
                         {
                             if (const auto existing = singletons_.find(key); existing != singletons_.end())
                             {
                                 return *created_services_[existing->second];
                             }
                         }

                         const auto &created = created_services_.emplace_back(factory.create_service(*this));
                         if (!std::holds_alternative<TransientScope>(factory.lifetime()))
                         {
                             singletons_.emplace(key, created_services_.size() - 1);
                         }
                         return *created;
                     }},
            call_site);
    }

    bool ScopedServiceProvider::can_resolve(const ServiceCallSite &call_site) const
    {
        return std::visit(Overload{[this](const InstanceServiceCallSite &instance)
                                   { return instance.registration_depth() == scope_level_; },
                                   [this](const FactoryServiceCallSite &factory)
                                   {
                                       return std::visit(Overload{[this](SingletonServiceLifetime)
                                                                  { return parent_scope_ == nullptr; },
                                                                  [this](const ScopedServiceLifetime &scope)
                                                                  { return scope.tag.is_none() || scope.tag == tag_; },
                                                                  [](TransientScope)
                                                                  {
                                                                      return true;
                                                                  }},
                                                         factory.lifetime());
                                   }},
                          call_site);
    }
} // namespace retro
