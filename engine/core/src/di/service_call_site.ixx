/**
 * @file service_call_site.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_call_site;

import std;
import :service_instance;
import retro.core.strings.name;
import retro.core.type_traits.pointer;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export struct SingletonScope
    {
    };

    export struct ScopedScope
    {
        Name tag;
    };

    export struct TransientScope
    {
    };

    export using ServiceLifetime = std::variant<SingletonScope, ScopedScope, TransientScope>;

    using ServiceFactory = std::move_only_function<std::shared_ptr<ServiceInstance>(class ServiceProvider &)>;

    template <typename T>
    concept ValidServiceFactoryResult =
        UniquePtrLike<T> || SharedPtrLike<T> || RefCountPtrLike<T> ||
        (std::is_pointer_v<T> && RefCounted<std::remove_pointer_t<T>>) || SmartHandle<T> || std::movable<T>;

    template <ValidServiceFactoryResult T>
    using ServiceInjectionType =
        std::conditional_t<UniquePtrLike<T> || SharedPtrLike<T> || RefCountPtrLike<T>,
                           PointerElementT<T>,
                           std::conditional_t<std::is_pointer_v<T> && RefCounted<std::remove_pointer_t<T>>,
                                              std::remove_pointer_t<T>,
                                              std::conditional_t<SmartHandle<T>, HandleElementType<T>, T>>>;

    template <typename T>
    concept CanCreateServiceFactoryFrom =
        std::invocable<T, ServiceProvider &> && ValidServiceFactoryResult<std::invoke_result_t<T, ServiceProvider &>>;

    template <CanCreateServiceFactoryFrom T>
    using ServiceResultType = ServiceInjectionType<std::invoke_result_t<T, ServiceProvider &>>;

    template <CanCreateServiceFactoryFrom Factory>
    std::shared_ptr<ServiceFactory> create_service_factory(Factory &&factory)
    {
        using Result = std::invoke_result_t<Factory, ServiceProvider &>;
        return std::make_shared<ServiceFactory>(
            [factory = std::forward<Factory>(factory)](ServiceProvider &provider)
            {
                if constexpr (UniquePtrLike<Result>)
                {
                    return ServiceInstance::from_unique(factory(provider));
                }
                else if constexpr (SharedPtrLike<Result>)
                {
                    return ServiceInstance::from_shared(factory(provider));
                }
                else if constexpr (RefCountPtrLike<Result> ||
                                   (std::is_pointer_v<Result> && RefCounted<std::remove_pointer_t<Result>>))
                {
                    return ServiceInstance::from_intrusive(factory(provider));
                }
                else if constexpr (SmartHandle<Result>)
                {
                    return ServiceInstance::from_smart_handle(factory(provider));
                }
                else
                {
                    return ServiceInstance::from_raw(factory(provider));
                }
            });
    }

    class InstanceServiceCallSite
    {
      public:
        explicit inline InstanceServiceCallSite(std::uint32_t depth, std::shared_ptr<ServiceInstance> instance) noexcept
            : registration_depth_{depth}, instance_{std::move(instance)}
        {
        }

        [[nodiscard]] inline std::uint32_t registration_depth() const noexcept
        {
            return registration_depth_;
        }

        [[nodiscard]] inline const ServiceInstance &instance() const noexcept
        {
            return *instance_;
        }

      private:
        std::uint32_t registration_depth_ = 0;
        std::shared_ptr<ServiceInstance> instance_;
    };

    class FactoryServiceCallSite
    {
      public:
        template <CanCreateServiceFactoryFrom Factory>
        explicit inline FactoryServiceCallSite(const ServiceLifetime lifetime, Factory &&factory) noexcept
            : lifetime_{lifetime}, service_type_{typeid(ServiceResultType<Factory>)},
              factory_{create_service_factory(std::forward<Factory>(factory))}
        {
        }

        template <typename T, CanCreateServiceFactoryFrom Factory>
            requires std::derived_from<ServiceResultType<Factory>, T>
        explicit inline FactoryServiceCallSite(const ServiceLifetime lifetime,
                                               std::in_place_type_t<T>,
                                               Factory &&factory) noexcept
            : lifetime_{lifetime}, service_type_{typeid(ServiceResultType<Factory>)},
              factory_{create_service_factory(std::forward<Factory>(factory))}
        {
        }

        [[nodiscard]] inline const ServiceLifetime &lifetime() const noexcept
        {
            return lifetime_;
        }

        [[nodiscard]] inline const std::type_index &service_type() const noexcept
        {
            return service_type_;
        }

        [[nodiscard]] inline std::shared_ptr<ServiceInstance> create_service(ServiceProvider &provider) const
        {
            return factory_->operator()(provider);
        }

      private:
        ServiceLifetime lifetime_;
        std::type_index service_type_ = typeid(void);
        std::shared_ptr<ServiceFactory> factory_;
    };

    using ServiceCallSite = std::variant<InstanceServiceCallSite, FactoryServiceCallSite>;
} // namespace retro
