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
        explicit ScopedServiceProvider(std::span<const ServiceRegistration> registrations);

        explicit ScopedServiceProvider(std::span<const ServiceRegistration> registrations,
                                       Name tag,
                                       std::shared_ptr<ServiceScope> parent_scope,
                                       ScopingRules rules);

        ScopedServiceProvider(const ScopedServiceProvider &) = delete;
        ScopedServiceProvider(ScopedServiceProvider &&) noexcept = default;

        ~ScopedServiceProvider() noexcept override;

        ScopedServiceProvider &operator=(const ScopedServiceProvider &) = delete;
        ScopedServiceProvider &operator=(ScopedServiceProvider &&) noexcept = default;

        void *get_raw(const std::type_info &type) override;

        std::generator<void *> get_all(const std::type_info &type) override;

        Name name() override;

        ServiceProvider &service_provider() override;

        const ScopingRules &scoping_rules() const override;

        std::uint32_t scope_level() const override;

        bool is_root_scope() const override;

        std::shared_ptr<ServiceScope> parent_scope() const override;

        std::shared_ptr<ServiceScope> create_scope() override;

        std::shared_ptr<ServiceScope> create_scope(Name name) override;

        std::shared_ptr<ServiceScope> create_scope(const Delegate<void(ServiceCollection &)> &configure) override;

        std::shared_ptr<ServiceScope> create_scope(Name name,
                                                   const Delegate<void(ServiceCollection &)> &configure) override;

        std::shared_ptr<ServiceScope> create_scope(const ScopingRules &rules) override;

        std::shared_ptr<ServiceScope> create_scope(Name name, const ScopingRules &rules) override;

        std::shared_ptr<ServiceScope> create_scope(const ScopingRules &rules,
                                                   const Delegate<void(ServiceCollection &)> &configure) override;

        std::shared_ptr<ServiceScope> create_scope(Name name,
                                                   const ScopingRules &rules,
                                                   const Delegate<void(ServiceCollection &)> &configure) override;

      private:
        const ServiceInstance &get_or_create(ServiceCallSite &call_site);

        Name tag_;
        std::shared_ptr<ServiceScope> parent_scope_;
        ScopingRules rules_;
        std::uint32_t scope_level_{0};
        std::vector<ServiceRegistration> service_registrations_;
        std::vector<ServiceInstance> created_services_;
        std::unordered_map<ServiceCacheKey, ServiceCallSite> services_;
    };
} // namespace retro
