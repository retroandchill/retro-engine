/**
 * @file scoped_service_provider.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:scoped_service_provider;

import :service_provider;
import :service_scope;

namespace retro
{
    class ScopedServiceProvider final : public ServiceProvider,
                                        public ServiceScope,
                                        public ServiceScopeFactory,
                                        public std::enable_shared_from_this<ScopedServiceProvider>
    {
      public:
        explicit ScopedServiceProvider(std::span<const ServiceCallSite> registrations);

        explicit ScopedServiceProvider(std::span<const ServiceCallSite> registrations,
                                       Name tag,
                                       std::shared_ptr<ServiceScope> parent_scope);

        ScopedServiceProvider(const ScopedServiceProvider &) = delete;
        ScopedServiceProvider(ScopedServiceProvider &&) noexcept = default;

        ~ScopedServiceProvider() noexcept override;

        ScopedServiceProvider &operator=(const ScopedServiceProvider &) = delete;
        ScopedServiceProvider &operator=(ScopedServiceProvider &&) noexcept = default;

        void *get_raw(const std::type_info &type) override;

        std::generator<void *> get_all(const std::type_info &type) override;

        Name name() override;

        ServiceProvider &service_provider() override;

        std::uint32_t scope_level() const override;

        bool is_root_scope() const override;

        std::shared_ptr<ServiceScope> parent_scope() const override;

        std::shared_ptr<ServiceScope> create_scope() override;

        std::shared_ptr<ServiceScope> create_scope(Name name) override;

        std::shared_ptr<ServiceScope> create_scope(const Delegate<void(ServiceCollection &)> &configure) override;

        std::shared_ptr<ServiceScope> create_scope(Name name,
                                                   const Delegate<void(ServiceCollection &)> &configure) override;

      private:
        const ServiceInstance &get_or_create(const ServiceCacheKey &key, const ServiceCallSite &call_site);

        bool can_resolve(const ServiceCallSite &call_site) const;

        Name tag_;
        std::shared_ptr<ServiceScope> parent_scope_;
        std::uint32_t scope_level_{0};
        std::unordered_map<ServiceCacheKey, std::size_t> registration_indices_;
        std::vector<ServiceCallSite> service_registrations_;
        std::unordered_map<ServiceCacheKey, std::size_t> singletons_;
        std::vector<std::shared_ptr<ServiceInstance>> created_services_;
    };
} // namespace retro
