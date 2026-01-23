/**
 * @file di.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core:di;

import std;
import :defines;
import :algorithm;
export import :di.metadata;
import :concepts;

namespace retro
{

    export class RETRO_API ServiceNotFoundException : public std::exception
    {
      public:
        [[nodiscard]] const char *what() const noexcept override;
    };

    export class ServiceCollection;
    export class ServiceProvider;

    using ServiceCreator = std::shared_ptr<void> (*)(ServiceProvider &);

    export enum class ServiceLifetime : uint8
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
        uint32 slot{};

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

    struct RealizedSingleton
    {
        std::shared_ptr<void> ptr{};
    };

    struct UnrealizedService
    {
        ServiceCreator registration{};
        bool is_singleton{};
    };

    using ServiceCallSite = std::variant<RealizedSingleton, UnrealizedService>;

    class RETRO_API ServiceProvider
    {
      public:
        explicit ServiceProvider(ServiceCollection &service_collection);

        template <typename T>
        decltype(auto) get()
        {
            if constexpr (SharedPtr<T>)
            {
                return std::static_pointer_cast<SharedPtrElement<T>>(get_shared_impl(typeid(SharedPtrElement<T>)));
            }
            else
            {
                return *get_ptr<T>();
            }
        }

      private:
        void *get_raw(const std::type_info &type);
        std::shared_ptr<void> get_shared_impl(const std::type_info &type);

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

        std::unordered_map<ServiceCacheKey, ServiceCallSite> services_;
    };

    struct ServiceRegistration
    {
        std::type_index type;
        ServiceCallSite registration;

        ServiceRegistration(const std::type_info &type, ServiceCreator factory, bool is_singleton) noexcept;
        ServiceRegistration(const std::type_info &type, std::shared_ptr<void> ptr) noexcept;
    };

    template <Injectable T>
    std::shared_ptr<void> construct_injectable(ServiceProvider &provider)
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

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = ServiceCreator;

        template <typename T>
        using TypedFactory = std::function<std::shared_ptr<T>(ServiceProvider &)>;

        template <ServiceLifetime Lifetime, typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        void add()
        {
            constexpr bool is_singleton = (Lifetime == ServiceLifetime::Singleton);

            if constexpr (is_singleton)
            {
                registrations_.emplace_back(typeid(T), &construct_injectable<Impl>, true);
            }
            else
            {
                registrations_.emplace_back(typeid(T), &construct_injectable<Impl>, true);
            }
        }

        template <typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        void add_singleton()
        {
            return add<ServiceLifetime::Singleton, T, Impl>();
        }

        template <typename T>
        void add_singleton(std::shared_ptr<T> ptr)
        {
            registrations_.emplace_back(typeid(T), std::move(ptr));
        }

        template <typename T, std::derived_from<T> Impl = T>
        void add_singleton(std::shared_ptr<Impl> ptr)
        {
            registrations_.emplace_back(typeid(T), std::move(ptr));
        }

        template <typename T>
        void add_singleton(std::unique_ptr<T> ptr)
        {
            registrations_.emplace_back(typeid(T), std::shared_ptr<T>(ptr.release()));
        }

        template <typename T, std::derived_from<T> Impl = T>
        void add_singleton(std::unique_ptr<Impl> ptr)
        {
            registrations_.emplace_back(typeid(T), std::shared_ptr<Impl>(ptr.release()));
        }

        template <typename T, std::derived_from<T> Impl = T>
            requires Injectable<Impl>
        void add_transient()
        {
            return add<ServiceLifetime::Transient, T, Impl>();
        }

      private:
        friend class ServiceProvider;

        std::vector<ServiceRegistration> registrations_;
    };
} // namespace retro
