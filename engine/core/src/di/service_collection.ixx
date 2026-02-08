/**
 * @file service_collection.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.di:service_collection;

import std;
import :service_provider;
import retro.core.type_traits.callable;

namespace retro
{
    template <Injectable T>
    std::unique_ptr<T> construct_unique_singleton(ServiceProvider &provider)
    {
        if constexpr (HasDependencies<T>)
        {
            return TypeListApply<SelectedCtorArgs<T>>::call(
                [&]<typename... Deps>() { return std::make_unique<T>(provider.get<std::decay_t<Deps>>()...); });
        }
        else
        {
            return std::make_unique<T>();
        }
    }

    template <Injectable T>
    std::shared_ptr<T> construct_shared_singleton(ServiceProvider &provider)
    {
        if constexpr (HasDependencies<T>)
        {
            return TypeListApply<SelectedCtorArgs<T>>::call(
                [&]<typename... Deps>() { return std::make_shared<T>(provider.get<std::decay_t<Deps>>()...); });
        }
        else
        {
            return std::make_shared<T>();
        }
    }

    template <Injectable T>
        requires RefCounted<T>
    RefCountPtr<T> construct_intrusive_singleton(ServiceProvider &provider)
    {
        if constexpr (HasDependencies<T>)
        {
            return TypeListApply<SelectedCtorArgs<T>>::call(
                [&]<typename... Deps>() { return make_ref_counted<T>(provider.get<std::decay_t<Deps>>()...); });
        }
        else
        {
            return make_ref_counted<T>();
        }
    }

    template <typename T, std::size_t... Is>
    T get_tuple_from_provider(ServiceProvider &provider, std::index_sequence<Is...>)
    {
        return {provider.get<std::decay_t<std::tuple_element_t<Is, T>>>()...};
    }

    template <typename T>
    T get_tuple_from_provider(ServiceProvider &provider)
    {
        return get_tuple_from_provider<T>(provider, std::make_index_sequence<std::tuple_size_v<std::decay_t<T>>>());
    }

    template <Injectable T, StoragePolicy Policy>
    ServiceInstance construct_service(ServiceProvider &provider)
    {
        if constexpr (Policy == StoragePolicy::UniqueOwned)
        {
            return ServiceInstance::from_unique(construct_unique_singleton<T>(provider));
        }
        else if constexpr (Policy == StoragePolicy::SharedOwned)
        {
            return ServiceInstance::from_shared(construct_shared_singleton<T>(provider));
        }
        else
        {
            static_assert(Policy == StoragePolicy::IntrusiveOwned);
            return ServiceInstance::from_intrusive(construct_intrusive_singleton<T>(provider));
        }
    }

    template <typename T, StoragePolicy Policy>
    concept InjectablePolicy = Injectable<T> && Policy != StoragePolicy::External &&
                               (Policy != StoragePolicy::IntrusiveOwned || RefCounted<T>);

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = ServiceFactory;

        template <typename T>
        using TypedFactory = std::function<std::shared_ptr<T>(ServiceProvider &)>;

        ServiceCollection() = default;

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, ServiceCallSite>
        explicit ServiceCollection(Range &&range, std::uint32_t registration_depth = 0)
            : registration_depth_(registration_depth), registrations_(std::from_range, std::forward<Range>(range))
        {
        }

        [[nodiscard]] inline std::span<const ServiceCallSite> registrations() const
        {
            return registrations_;
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add(ServiceLifetime lifetime)
        {
            return add<T, T, Policy>(lifetime);
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add(ServiceLifetime lifetime)
        {
            registrations_.emplace_back(lifetime, std::in_place_type<T>, &construct_service<Impl, Policy>);
            return *this;
        }

        template <CanCreateServiceFactoryFrom Functor>
        ServiceCollection &add(ServiceLifetime lifetime, Functor &&functor)
        {
            registrations_.emplace_back(std::in_place_type<FactoryServiceCallSite>,
                                        lifetime,
                                        std::forward<Functor>(functor));
            return *this;
        }

        template <auto Functor>
            requires(CanCreateServiceFactoryFrom<decltype(Functor)>)
        ServiceCollection &add(ServiceLifetime lifetime)
        {
            return add(lifetime, [](ServiceProvider &provider) { return std::invoke(Functor, provider); });
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceFactoryResult<FunctionReturnType<Functor>> && !CanCreateServiceFactoryFrom<Functor>)
        ServiceCollection &add(ServiceLifetime lifetime, Functor &&functor)
        {
            return add(lifetime,
                       [functor = std::forward<Functor>(functor)](ServiceProvider &provider)
                       { return std::apply(functor, get_tuple_from_provider<FunctionArgsTuple<Functor>>(provider)); });
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> &&
                     ValidServiceFactoryResult<FunctionReturnType<decltype(Functor)>> &&
                     !CanCreateServiceFactoryFrom<decltype(Functor)>)
        ServiceCollection &add(ServiceLifetime lifetime)
        {
            return add(lifetime,
                       [](ServiceProvider &provider) {
                           return std::apply(Functor,
                                             get_tuple_from_provider<FunctionArgsTuple<decltype(Functor)>>(provider));
                       });
        }

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        ServiceCollection &add(std::in_place_t, Args &&...args)
        {
            return add<T, T>(std::in_place, std::forward<Args>(args)...);
        }

        template <typename T, std::derived_from<T> Impl, typename... Args>
            requires std::constructible_from<Impl, Args...>
        ServiceCollection &add(std::in_place_t, Args &&...args)
        {
            registrations_.emplace_back(registration_depth_,
                                        ServiceInstance::from_direct<T, Impl>(std::forward<Args>(args)...));
            return *this;
        }

        template <typename T>
        ServiceCollection &add(std::shared_ptr<T> ptr)
        {
            registrations_.emplace_back(std::in_place_type<InstanceServiceCallSite>,
                                        registration_depth_,
                                        ServiceInstance::from_shared<T>(std::move(ptr)));
            return *this;
        }

        template <typename T, std::invocable<T *> Deleter>
        ServiceCollection &add(std::unique_ptr<T, Deleter> ptr)
        {
            registrations_.emplace_back(std::in_place_type<InstanceServiceCallSite>,
                                        registration_depth_,
                                        ServiceInstance::from_unique<T>(std::move(ptr)));
            return *this;
        }

        template <typename T, std::derived_from<T> Impl = T>
        ServiceCollection &add_singleton(std::unique_ptr<Impl> ptr)
        {
            registrations_.emplace_back(typeid(T), std::shared_ptr<Impl>(ptr.release()));
            return *this;
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add_singleton()
        {
            return add<T, T, Policy>(SingletonServiceLifetime{});
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add_singleton()
        {
            return add<T, Impl, Policy>(SingletonServiceLifetime{});
        }

        template <CanCreateServiceFactoryFrom Functor>
        ServiceCollection &add_singleton(Functor &&functor)
        {
            return add(SingletonServiceLifetime{}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires CanCreateServiceFactoryFrom<decltype(Functor)>
        ServiceCollection &add_singleton()
        {
            return add<Functor>(SingletonServiceLifetime{});
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceFactoryResult<FunctionReturnType<Functor>> && !CanCreateServiceFactoryFrom<Functor>)
        ServiceCollection &add_singleton(Functor &&functor)
        {
            return add(SingletonServiceLifetime{}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> &&
                     ValidServiceFactoryResult<FunctionReturnType<decltype(Functor)>> &&
                     !CanCreateServiceFactoryFrom<decltype(Functor)>)
        ServiceCollection &add_singleton()
        {
            return add<Functor>(SingletonServiceLifetime{});
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add_scoped(const Name scope_tag = Name::none())
        {
            return add<T, T, Policy>(ScopedServiceLifetime{scope_tag});
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add_scoped(const Name scope_tag = Name::none())
        {
            return add<T, Impl, Policy>(ScopedServiceLifetime{scope_tag});
        }

        template <CanCreateServiceFactoryFrom Functor>
        ServiceCollection &add_scoped(Functor &&functor)
        {
            return add_scoped(Name::none(), std::forward<Functor>(functor));
        }

        template <CanCreateServiceFactoryFrom Functor>
        ServiceCollection &add_scoped(const Name scope_tag, Functor &&functor)
        {
            return add(ScopedServiceLifetime{scope_tag}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires std::invocable<decltype(Functor), ServiceProvider &> &&
                     CanCreateServiceFactoryFrom<decltype(Functor)>
        ServiceCollection &add_scoped(const Name scope_tag = Name::none())
        {
            return add<Functor>(ScopedServiceLifetime{scope_tag});
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceFactoryResult<FunctionReturnType<Functor>> && !CanCreateServiceFactoryFrom<Functor>)
        ServiceCollection &add_scoped(Functor &&functor)
        {
            return add_scoped(Name::none(), std::forward<Functor>(functor));
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceFactoryResult<FunctionReturnType<Functor>> && !CanCreateServiceFactoryFrom<Functor>)
        ServiceCollection &add_scoped(const Name scope_tag, Functor &&functor)
        {
            return add(ScopedServiceLifetime{scope_tag}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> &&
                     ValidServiceFactoryResult<FunctionReturnType<decltype(Functor)>> &&
                     !CanCreateServiceFactoryFrom<decltype(Functor)>)
        ServiceCollection &add_scoped(const Name scope_tag = Name::none())
        {
            return add<Functor>(ScopedServiceLifetime{scope_tag});
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add_transient()
        {
            return add<T, T, Policy>(TransientScope{});
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add_transient()
        {
            return add<T, Impl, Policy>(TransientScope{});
        }

        template <CanCreateServiceFactoryFrom Functor>
        ServiceCollection &add_transient(Functor &&functor)
        {
            return add(TransientScope{}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires std::invocable<decltype(Functor), ServiceProvider &> &&
                     CanCreateServiceFactoryFrom<decltype(Functor)>
        ServiceCollection &add_transient()
        {
            return add<Functor>(TransientScope{});
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceFactoryResult<FunctionReturnType<Functor>> && !CanCreateServiceFactoryFrom<Functor>)
        ServiceCollection &add_transient(Functor &&functor)
        {
            return add(TransientScope{}, std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> &&
                     ValidServiceFactoryResult<FunctionReturnType<decltype(Functor)>> &&
                     !CanCreateServiceFactoryFrom<decltype(Functor)>)
        ServiceCollection &add_transient()
        {
            return add<Functor>(TransientScope{});
        }

        [[nodiscard]] std::shared_ptr<ServiceProvider> create_service_provider() const;

      private:
        std::uint32_t registration_depth_{0};
        std::vector<ServiceCallSite> registrations_;
    };
} // namespace retro
