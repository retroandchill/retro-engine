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
        explicit ScopedServiceProvider(Name tag = Name::none(), std::shared_ptr<ServiceScope> parent_scope = nullptr);

        void *get_raw(const std::type_info &type) override;

        std::generator<void *> get_all(const std::type_info &type) override;

        Name name() override;

        ServiceProvider &service_provider() override;

        std::shared_ptr<ServiceScope> create_scope() override;

        std::shared_ptr<ServiceScope> create_scope(Name name) override;

        std::shared_ptr<ServiceScope> create_scope(Delegate<void(ServiceCollection &)> configure) override;

        std::shared_ptr<ServiceScope> create_scope(Name name, Delegate<void(ServiceCollection &)> configure) override;

      private:
        const ServiceInstance &get_or_create(std::type_index type, ServiceCallSite &call_site);

        Name tag_;
        std::shared_ptr<ServiceScope> parent_scope_;
        std::vector<ServiceInstance> created_services_;
        std::unordered_map<ServiceCacheKey, ServiceCallSite> services_;
    };
} // namespace retro
