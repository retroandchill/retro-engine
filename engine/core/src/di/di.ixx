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

namespace retro
{
    export class RETRO_API ServiceNotFoundException : public std::exception
    {
      public:
        [[nodiscard]] const char *what() const noexcept override;
    };

    export class ServiceProvider;
    export class ServiceCollection;

    export enum class ServiceType : uint8
    {
        Singleton,
        Transient
    };

    using ServiceFactory = std::function<std::shared_ptr<void>(ServiceProvider &)>;

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
        ServiceFactory registration{};
        bool is_singleton{};
    };

    using ServiceCallSite = std::variant<RealizedSingleton, UnrealizedService>;

    class RETRO_API ServiceProvider
    {
      public:
        explicit ServiceProvider(ServiceCollection &service_collection);

        template <typename T>
        T &get()
        {
            return *get_ptr<T>();
        }

        template <typename T>
        std::shared_ptr<T> get_shared()
        {
            return get_shared_impl(typeid(T));
        }

        template <typename T>
        std::vector<T *> get_all()
        {
            return get_all(typeid(T)) |
                   std::views::transform([](const auto &ptr) { return static_cast<T *>(ptr.get()); }) |
                   std::ranges::to<std::vector>();
        }

        template <typename T>
        std::vector<std::shared_ptr<T>> get_all_shared()
        {
            return get_all(typeid(T)) |
                   std::views::transform([](const auto &ptr) { return std::static_pointer_cast<T>(ptr.get()); }) |
                   std::ranges::to<std::vector>();
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

        ServiceRegistration(const std::type_info &type, ServiceFactory factory, bool is_singleton) noexcept;
        ServiceRegistration(const std::type_info &type, std::shared_ptr<void> ptr) noexcept;
    };

    export class RETRO_API ServiceCollection
    {
      public:
        using Factory = ServiceFactory;

        template <typename T>
        using TypedFactory = std::function<std::shared_ptr<T>(ServiceProvider &)>;

        template <typename T>
        void add_singleton(TypedFactory<T> factory)
        {
            add_singleton(typeid(T), [f = std::move(factory)](ServiceProvider &provider) { return f(provider); });
        }

        template <typename T>
        void add_singleton(std::shared_ptr<T> ptr)
        {
            return add_singleton(typeid(T), std::move(ptr));
        }

        template <typename T>
        void add_transient(TypedFactory<T> factory)
        {
            add_transient(typeid(T), [f = std::move(factory)](ServiceProvider &provider) { return f(provider); });
        }

      private:
        void add_singleton(const std::type_info &type, Factory factory);
        void add_singleton(const std::type_info &type, std::shared_ptr<void> ptr);
        void add_transient(const std::type_info &type, Factory factory);

        friend class ServiceProvider;

        std::vector<ServiceRegistration> registrations_;
    };
} // namespace retro
