/**
 * @file service_provider.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.di:service_provider;

import std;

import retro.core.algorithm.hashing;
import retro.core.type_traits.pointer;
import retro.core.type_traits.range;
import retro.core.functional.overload;
import :metadata;
import :service_instance;
import :service_call_site;
import :service_identifier;
import retro.core.functional.delegate;

namespace retro
{
    export class RETRO_API ServiceNotFoundException : public std::exception
    {
      public:
        [[nodiscard]] const char *what() const noexcept override;
    };

    export class ServiceProvider;

    export enum class ServiceLifetime : std::uint8_t
    {
        Singleton,
        Transient
    };

    template <Injectable T>
    void *construct_transient(ServiceProvider &provider);

    template <Injectable T>
    T construct_transient_in_place(ServiceProvider &provider);

    template <typename T>
    concept ServiceCompatibleContainer =
        std::ranges::range<T> && ContainerAppendable<T, PointerElement<std::ranges::range_reference_t<T>>> &&
        (SharedPtrLike<std::ranges::range_reference_t<T>> || std::is_pointer_v<std::ranges::range_value_t<T>>);

    class RETRO_API ServiceProvider
    {
      public:
        explicit ServiceProvider(class ServiceCollection &service_collection);

        ServiceProvider(const ServiceProvider &) = delete;
        ServiceProvider(ServiceProvider &&) noexcept = default;

        ~ServiceProvider() noexcept;

        ServiceProvider &operator=(const ServiceProvider &) = delete;
        ServiceProvider &operator=(ServiceProvider &&) noexcept = default;

        template <typename T>
        decltype(auto) get()
        {
            if constexpr (ServiceCompatibleContainer<std::decay_t<T>>)
            {
                using DecayedT = std::decay_t<T>;
                using ElementType = PointerElementT<std::ranges::range_reference_t<DecayedT>>;
                if constexpr (SharedPtrLike<std::ranges::range_reference_t<DecayedT>>)
                {
                    return get_all(typeid(ElementType)) |
                           std::views::filter([](const ServiceInstance &instance)
                                              { return instance.has_shared_storage(); }) |
                           std::views::transform(
                               [](const ServiceInstance &instance)
                               { return std::static_pointer_cast<ElementType>(instance.shared_ptr()); }) |
                           std::ranges::to<DecayedT>();
                }
                else
                {
                    return get_all(typeid(ElementType)) |
                           std::views::filter([](const ServiceInstance &instance)
                                              { return instance.has_shared_storage(); }) |
                           std::views::transform([](const ServiceInstance &instance)
                                                 { return instance.get<ElementType>(); }) |
                           std::ranges::to<DecayedT>();
                }
            }
            else if constexpr (SharedPtrLike<T>)
            {
                return std::static_pointer_cast<PointerElementT<T>>(get_shared_impl(typeid(PointerElementT<T>)));
            }
            else
            {
                return *get_ptr<T>();
            }
        }

        template <typename T>
        auto create()
        {
            if (UniquePtrLike<T>)
            {
                return std::unique_ptr<PointerElementT<T>>(create_raw<PointerElementT<T>>(typeid(PointerElementT<T>)));
            }
            // ReSharper disable once CppRedundantElseKeywordInsideCompoundStatement
            else if constexpr (SharedPtrLike<T>)
            {
                return std::shared_ptr<PointerElementT<T>>(create_raw<PointerElementT<T>>(typeid(PointerElementT<T>)));
            }
            else
            {
                auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{typeid(T)}});
                if (existing != services_.end())
                {
                    std::visit(Overload{[&](const DirectTransient) -> T
                                        { return construct_transient_in_place<T>(*this); },
                                        [](auto &&) -> T
                                        {
                                            throw ServiceNotFoundException{};
                                        }},
                               existing->second);
                }

                throw ServiceNotFoundException{};
            }
        }

      private:
        void *get_raw(const std::type_info &type);

        std::shared_ptr<void> get_shared_impl(const std::type_info &type);

        template <typename T>
        T *create_raw(const std::type_info &type)
        {
            auto existing = services_.find(ServiceCacheKey{.id = ServiceIdentifier{type}});
            if (existing != services_.end())
            {
                auto *created = std::visit(
                    Overload{[](const RealizedSingleton &) -> void * { throw ServiceNotFoundException{}; },
                             [](const UnrealizedSingleton) -> void * { throw ServiceNotFoundException{}; },
                             [&](const DerivedTransient &transient) { return transient.registration.execute(*this); },
                             [&](const DirectTransient) -> void *
                             {
                                 if constexpr (Injectable<T>)
                                 {
                                     return construct_transient<T>(*this);
                                 }
                                 else
                                 {
                                     throw ServiceNotFoundException{};
                                 }
                             }},
                    existing->second);
                return static_cast<T *>(created);
            }

            throw ServiceNotFoundException{};
        }

        auto get_all(const std::type_info &type)
        {
            using Pair = decltype(services_)::value_type;
            return services_ | std::views::filter([&type](const Pair &pair) { return pair.first.id.type == type; }) |
                   std::views::values |
                   std::views::transform([this](ServiceCallSite &call_site) -> auto &
                                         { return get_or_create(call_site); });
        }

        const ServiceInstance &get_or_create(ServiceCallSite &call_site);

        template <typename T>
        T *get_ptr()
        {
            return static_cast<T *>(get_raw(typeid(T)));
        }

        std::vector<ServiceInstance> singletons_;
        std::unordered_map<ServiceCacheKey, ServiceCallSite> services_;
    };

} // namespace retro
