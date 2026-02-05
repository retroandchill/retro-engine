/**
 * @file di.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.di;

import std;

import retro.core.algorithm.hashing;
import retro.core.type_traits.pointer;
import retro.core.type_traits.range;
import retro.core.functional.overload;
export import :metadata;
import retro.core.functional.delegate;

namespace retro
{
    export class RETRO_API ServiceNotFoundException : public std::exception
    {
      public:
        [[nodiscard]] const char *what() const noexcept override;
    };

    export class ServiceCollection;
    export class ServiceProvider;

    export enum class ServiceLifetime : std::uint8_t
    {
        Singleton,
        Transient
    };

    struct ServiceIdentifier
    {
        std::type_index type;

        explicit inline ServiceIdentifier(const std::type_info &type) noexcept : type(type)
        {
        }

        explicit inline ServiceIdentifier(const std::type_index &type) noexcept : type(type)
        {
        }

        friend bool operator==(const ServiceIdentifier &lhs, const ServiceIdentifier &rhs) noexcept = default;
    };

    struct ServiceCacheKey
    {
        ServiceIdentifier id;
        std::uint32_t slot{};

        friend bool operator==(const ServiceCacheKey &lhs, const ServiceCacheKey &rhs) noexcept = default;
    };
} // namespace retro

template <>
struct std::hash<retro::ServiceIdentifier>
{
    [[nodiscard]] inline size_t operator()(const retro::ServiceIdentifier &id) const noexcept
    {
        return id.type.hash_code();
    }
};

template <>
struct std::hash<retro::ServiceCacheKey>
{
    [[nodiscard]] inline size_t operator()(const retro::ServiceCacheKey &key) const noexcept
    {
        return retro::hash_combine(key.id, key.slot);
    }
};

namespace retro
{
    using SingletonCreator = std::shared_ptr<void> (*)(ServiceProvider &);
    using TransientCreator = void *(*)(ServiceProvider &);

    struct ExternalSingleton
    {
        std::shared_ptr<void> ptr{};
    };

    struct RealizedSingleton
    {
        std::weak_ptr<void> ptr{};
    };

    struct UnrealizedSingleton
    {
        SingletonCreator registration{};
    };

    struct DirectTransient
    {
    };

    struct DerivedTransient
    {
        TransientCreator registration{};
    };

    using ServiceCallSite =
        std::variant<ExternalSingleton, RealizedSingleton, UnrealizedSingleton, DirectTransient, DerivedTransient>;

    template <Injectable T>
    void *construct_transient(ServiceProvider &provider);

    template <Injectable T>
    T construct_transient_in_place(ServiceProvider &provider);

    template <typename T>
    concept ServiceCompatbleContainer = std::ranges::range<T> && SharedPtrLike<std::ranges::range_reference_t<T>> &&
                                        ContainerAppendable<T, PointerElement<std::ranges::range_reference_t<T>>>;

    class RETRO_API ServiceProvider
    {
      public:
        explicit ServiceProvider(ServiceCollection &service_collection);

        ServiceProvider(const ServiceProvider &) = default;
        ServiceProvider(ServiceProvider &&) noexcept = default;

        ~ServiceProvider() noexcept;

        ServiceProvider &operator=(const ServiceProvider &) = default;
        ServiceProvider &operator=(ServiceProvider &&) noexcept = default;

        template <typename T>
        decltype(auto) get()
        {
            if constexpr (ServiceCompatbleContainer<std::decay_t<T>>)
            {
                using DecayedT = std::decay_t<T>;
                using ElementType = PointerElementT<std::ranges::range_reference_t<DecayedT>>;
                return get_all(typeid(ElementType)) |
                       std::views::transform([](auto ptr) { return std::static_pointer_cast<ElementType>(ptr); }) |
                       std::ranges::to<DecayedT>();
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
                auto *created =
                    std::visit(Overload{[](const ExternalSingleton &) -> void * { throw ServiceNotFoundException{}; },
                                        [](const RealizedSingleton &) -> void * { throw ServiceNotFoundException{}; },
                                        [](const UnrealizedSingleton) -> void * { throw ServiceNotFoundException{}; },
                                        [&](const DerivedTransient transient) { return transient.registration(*this); },
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
                   std::views::transform([this](ServiceCallSite &call_site) { return get_or_create(call_site); });
        }

        std::shared_ptr<void> get_or_create(ServiceCallSite &call_site);

        template <typename T>
        T *get_ptr()
        {
            return static_cast<T *>(get_raw(typeid(T)));
        }

        std::vector<std::shared_ptr<void>> instantiated_singletons_;
        std::unordered_map<ServiceCacheKey, ServiceCallSite> services_;
    };

    struct RETRO_API ServiceRegistration
    {
        std::type_index type;
        ServiceCallSite registration;

        explicit ServiceRegistration(const std::type_info &type) noexcept;

        ServiceRegistration(const std::type_info &type, SingletonCreator factory) noexcept;

        ServiceRegistration(const std::type_info &type, TransientCreator factory) noexcept;

        ServiceRegistration(const std::type_info &type, std::shared_ptr<void> ptr) noexcept;
    };

    template <Injectable T>
    std::shared_ptr<void> construct_singleton(ServiceProvider &provider)
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

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = SingletonCreator;

        template <typename T>
        using TypedFactory = std::function<std::shared_ptr<T>(ServiceProvider &)>;

        template <ServiceLifetime Lifetime, typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        ServiceCollection &add()
        {
            if constexpr (Lifetime == ServiceLifetime::Singleton)
            {
                registrations_.emplace_back(typeid(T), &construct_singleton<Impl>);
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

        template <typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        ServiceCollection &add_singleton()
        {
            return add<ServiceLifetime::Singleton, T, Impl>();
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
