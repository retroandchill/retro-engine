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

    template <typename T>
    concept ValidServiceResult =
        UniquePtrLike<T> || SharedPtrLike<T> || SmartHandle<T> || RefCountPtrLike<T> || std::movable<T>;

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = ServiceFactory;

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
            registrations_.emplace_back(typeid(T), Lifetime, &construct_service<Impl, Policy>);
            return *this;
        }

        template <ServiceLifetime Lifetime, std::invocable<ServiceProvider &> Functor>
            requires ValidServiceResult<std::invoke_result_t<Functor, ServiceProvider &>>
        ServiceCollection &add(Functor &&functor)
        {
            using Result = std::invoke_result_t<Functor, ServiceProvider &>;
            if constexpr (UniquePtrLike<Result>)
            {
                using InnerType = PointerElementT<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    Lifetime,
                    ServiceFactory::create([factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                                           { return ServiceInstance::from_unique(std::invoke(factory, provider)); }));
            }
            else if constexpr (SharedPtrLike<Result>)
            {
                using InnerType = PointerElementT<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    Lifetime,
                    ServiceFactory::create([factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                                           { return ServiceInstance::from_shared(std::invoke(factory, provider)); }));
            }
            else if constexpr (RefCountPtrLike<Result>)
            {
                using InnerType = PointerElementT<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    Lifetime,
                    ServiceFactory::create(
                        [factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                        { return ServiceInstance::from_intrusive(std::invoke(factory, provider)); }));
            }
            else if constexpr (SmartHandle<Result>)
            {
                using InnerType = HandleElementType<Result>;
                registrations_.emplace_back(
                    typeid(InnerType),
                    Lifetime,
                    ServiceFactory::create(
                        [factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                        { return ServiceInstance::from_smart_handle(std::invoke(factory, provider)); }));
            }
            else
            {
                registrations_.emplace_back(
                    typeid(Result),
                    Lifetime,
                    ServiceFactory::create([factory = std::forward<Functor>(functor)](ServiceProvider &provider)
                                           { return ServiceInstance::from_direct(std::invoke(factory, provider)); }));
            }

            return *this;
        }

        template <ServiceLifetime Lifetime, auto Functor>
            requires(std::invocable<decltype(Functor), ServiceProvider &> &&
                     ValidServiceResult<std::invoke_result_t<decltype(Functor), ServiceProvider &>>)
        ServiceCollection &add()
        {
            return add<Lifetime>([](ServiceProvider &provider) { return std::invoke(Functor, provider); });
        }

        template <ServiceLifetime Lifetime, NonGenericLambda Functor>
            requires(ValidServiceResult<FunctionReturnType<Functor>> && !std::invocable<Functor, ServiceProvider &>)
        ServiceCollection &add(Functor &&functor)
        {
            return add<Lifetime>(
                [functor = std::forward<Functor>(functor)](ServiceProvider &provider)
                { return std::apply(functor, get_tuple_from_provider<FunctionArgsTuple<Functor>>(provider)); });
        }

        template <ServiceLifetime Lifetime, auto Functor>
            requires(FreeFunction<decltype(Functor)> && ValidServiceResult<FunctionReturnType<decltype(Functor)>> &&
                     !std::invocable<decltype(Functor), ServiceProvider &>)
        ServiceCollection &add()
        {
            return add<Lifetime>(
                [](ServiceProvider &provider) {
                    return std::apply(Functor, get_tuple_from_provider<FunctionArgsTuple<decltype(Functor)>>(provider));
                });
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
            requires ValidServiceResult<std::invoke_result_t<Functor, ServiceProvider &>>
        ServiceCollection &add_singleton(Functor &&functor)
        {
            return add<ServiceLifetime::Singleton>(std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires std::invocable<decltype(Functor), ServiceProvider &> &&
                     ValidServiceResult<std::invoke_result_t<decltype(Functor), ServiceProvider &>>
        ServiceCollection &add_singleton()
        {
            return add<ServiceLifetime::Singleton, Functor>();
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceResult<FunctionReturnType<Functor>> && !std::invocable<Functor, ServiceProvider &>)
        ServiceCollection &add_singleton(Functor &&functor)
        {
            return add<ServiceLifetime::Singleton>(std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> && ValidServiceResult<FunctionReturnType<decltype(Functor)>> &&
                     !std::invocable<decltype(Functor), ServiceProvider &>)
        ServiceCollection &add_singleton()
        {
            return add<ServiceLifetime::Singleton, Functor>();
        }

        template <typename T, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<T, Policy>
        ServiceCollection &add_transient()
        {
            return add<ServiceLifetime::Transient, T, T, Policy>();
        }

        template <typename T, std::derived_from<T> Impl, StoragePolicy Policy = StoragePolicy::UniqueOwned>
            requires InjectablePolicy<Impl, Policy>
        ServiceCollection &add_transient()
        {
            return add<ServiceLifetime::Transient, T, Impl, Policy>();
        }

        template <std::invocable<ServiceProvider &> Functor>
            requires ValidServiceResult<std::invoke_result_t<Functor, ServiceProvider &>>
        ServiceCollection &add_transient(Functor &&functor)
        {
            return add<ServiceLifetime::Transient>(std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires std::invocable<decltype(Functor), ServiceProvider &> &&
                     ValidServiceResult<std::invoke_result_t<decltype(Functor), ServiceProvider &>>
        ServiceCollection &add_transient()
        {
            return add<ServiceLifetime::Transient, Functor>();
        }

        template <NonGenericLambda Functor>
            requires(ValidServiceResult<FunctionReturnType<Functor>> && !std::invocable<Functor, ServiceProvider &>)
        ServiceCollection &add_transient(Functor &&functor)
        {
            return add<ServiceLifetime::Transient>(std::forward<Functor>(functor));
        }

        template <auto Functor>
            requires(FreeFunction<decltype(Functor)> && ValidServiceResult<FunctionReturnType<decltype(Functor)>> &&
                     !std::invocable<decltype(Functor), ServiceProvider &>)
        ServiceCollection &add_transient()
        {
            return add<ServiceLifetime::Transient, Functor>();
        }

        std::shared_ptr<ServiceProvider> create_service_provider() const;

      private:
        std::vector<ServiceRegistration> registrations_;
    };
} // namespace retro
