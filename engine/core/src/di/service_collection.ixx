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

namespace retro
{
    struct RETRO_API ServiceRegistration
    {
        std::type_index type;
        ServiceCallSite registration;

        explicit ServiceRegistration(const std::type_info &type) noexcept;

        ServiceRegistration(const std::type_info &type, SingletonFactory factory) noexcept;

        ServiceRegistration(const std::type_info &type, TransientFactory factory) noexcept;

        template <typename T>
        ServiceRegistration(const std::type_info &type, std::shared_ptr<T> ptr) noexcept
            : type{type}, registration{std::in_place_type<UnrealizedSingleton>,
                                       [p = std::move(ptr)](auto &)
                                       {
                                           return ServiceInstance::from_shared(std::move(p));
                                       }}
        {
        }

        template <typename T>
        ServiceRegistration(const std::type_info &type, std::unique_ptr<T> ptr) noexcept
            : type{type}, registration{std::in_place_type<RealizedSingleton>,
                                       [p = std::move(ptr)](auto &)
                                       {
                                           return ServiceInstance::from_unique(std::move(p));
                                       }}
        {
        }

        template <typename T>
        ServiceRegistration(const std::type_info &type, RefCountPtr<T> ptr) noexcept
            : type{type}, registration{std::in_place_type<RealizedSingleton>,
                                       [p = std::move(ptr)](auto &)
                                       {
                                           return ServiceInstance::from_intrusive(std::move(p));
                                       }}
        {
        }
    };

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

    template <Injectable T, StoragePolicy Policy>
    ServiceInstance construct_singleton(ServiceProvider &provider)
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

    template <Injectable T>
    void *construct_transient(ServiceProvider &provider)
    {
        if constexpr (HasDependencies<T>)
        {
            return TypeListApply<SelectedCtorArgs<T>>::call([&]<typename... Deps>()
                                                            { return new T{provider.get<std::decay_t<Deps>>()...}; });
        }
        else
        {
            return new T{};
        }
    }

    template <Injectable T>
    T construct_transient_in_place(ServiceProvider &provider)
    {
        if constexpr (HasDependencies<T>)
        {
            return TypeListApply<SelectedCtorArgs<T>>::call([&]<typename... Deps>()
                                                            { return T{provider.get<std::decay_t<Deps>>()...}; });
        }
        else
        {
            return T{};
        }
    }

    template <typename T, StoragePolicy Policy>
    concept InjectablePolicy = Injectable<T> && Policy != StoragePolicy::External &&
                               (Policy != StoragePolicy::IntrusiveOwned || RefCounted<T>);

    template <typename T>
    concept ValidSingletonResult = UniquePtrLike<T> || SharedPtrLike<T> || SmartHandle<T>;

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = SingletonFactory;

        template <typename T>
        using TypedFactory = std::function<std::shared_ptr<T>(ServiceProvider &)>;

        template <ServiceLifetime Lifetime, typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add()
        {
            return add<Lifetime, T, T, Policy>();
        }

        template <ServiceLifetime Lifetime,
                  typename T,
                  std::derived_from<T> Impl,
                  StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add()
        {
            if constexpr (Lifetime == ServiceLifetime::Singleton)
            {
                registrations_.emplace_back(typeid(T), &construct_singleton<Impl, Policy>);
            }
            else if constexpr (Lifetime == ServiceLifetime::Transient)
            {
                if (std::same_as<Impl, T>)
                {
                    registrations_.emplace_back(typeid(T));
                }
                else
                {
                    registrations_.emplace_back(typeid(T), &construct_transient<Impl>);
                }
            }

            return *this;
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add_singleton()
        {
            return add<ServiceLifetime::Singleton, T, T, Policy>();
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add_singleton()
        {
            return add<ServiceLifetime::Singleton, T, Impl, Policy>();
        }

        template <typename T>
        ServiceCollection &add_singleton(std::shared_ptr<T> ptr)
        {
            registrations_.emplace_back(typeid(T), std::move(ptr));
            return *this;
        }

        template <typename T, std::derived_from<T> Impl = T>
        ServiceCollection &add_singleton(std::shared_ptr<Impl> ptr)
        {
            registrations_.emplace_back(typeid(T), std::move(ptr));
            return *this;
        }

        template <typename T>
        ServiceCollection &add_singleton(std::unique_ptr<T> ptr)
        {
            registrations_.emplace_back(typeid(T), std::shared_ptr<T>(ptr.release()));
            return *this;
        }

        template <typename T, std::derived_from<T> Impl = T>
        ServiceCollection &add_singleton(std::unique_ptr<Impl> ptr)
        {
            registrations_.emplace_back(typeid(T), std::shared_ptr<Impl>(ptr.release()));
            return *this;
        }

        template <std::invocable<ServiceProvider &> Functor>
            requires ValidSingletonResult<std::invoke_result_t<Functor, ServiceProvider &>>
        ServiceCollection &add_singleton(Functor &&functor)
        {
            using Result = std::invoke_result_t<Functor, ServiceProvider &>;
            if constexpr (UniquePtrLike<Result>)
            {
                using InnerType = PointerElementT<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    SingletonFactory::create([factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                                             { return ServiceInstance::from_unique(std::invoke(factory, provider)); }));
            }
            else if constexpr (SharedPtrLike<Result>)
            {
                using InnerType = PointerElementT<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    SingletonFactory::create([factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                                             { return ServiceInstance::from_shared(std::invoke(factory, provider)); }));
            }
            else if constexpr (SmartHandle<Result>)
            {
                using InnerType = HandleElementType<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    SingletonFactory::create(
                        [factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                        { return ServiceInstance::from_smart_handle(std::invoke(factory, provider)); }));
            }

            return *this;
        }

        template <typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        ServiceCollection &add_transient()
        {
            return add<ServiceLifetime::Transient, T, Impl>();
        }

      private:
        friend class ServiceProvider;

        std::vector<ServiceRegistration> registrations_;
    };
} // namespace retro
