/**
 * @file service_instance.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_instance;

import std;
import retro.core.functional.delegate;
import retro.core.memory.ref_counted_ptr;
import retro.core.util.noncopyable;
import :metadata;

namespace retro
{
    export enum class StoragePolicy : std::uint8_t
    {
        external,
        direct,
        unique_owned,
        shared_owned,
        intrusive_owned
    };

    export class ServiceInstance : NonCopyable
    {
      protected:
        inline ServiceInstance(const std::type_index type, void *ptr) noexcept : type_(type), ptr_(ptr)
        {
        }

      public:
        virtual ~ServiceInstance() = default;

        [[nodiscard]] inline std::type_index type() const noexcept
        {
            return type_;
        }

        [[nodiscard]] inline void *ptr() const noexcept
        {
            return ptr_;
        }

        template <typename T>
        [[nodiscard]] T *get() const noexcept
        {
            return static_cast<T *>(ptr_);
        }

        [[nodiscard]] inline bool is_valid() const noexcept
        {
            return ptr_ != nullptr;
        }

        template <typename T>
            requires std::constructible_from<std::remove_cvref_t<T>, T>
        static std::shared_ptr<ServiceInstance> from_direct(T &&value)
        {
            return from_direct<T, T>(std::forward<T>(value));
        }

        template <typename T, std::derived_from<T> Impl>
            requires std::constructible_from<std::remove_cvref_t<Impl>, Impl>
        static std::shared_ptr<ServiceInstance> from_direct(Impl &&value);

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        static std::shared_ptr<ServiceInstance> from_direct(Args &&...args)
        {
            return from_direct<T, T>(std::forward<Args>(args)...);
        }

        template <typename T, std::derived_from<T> Impl, typename... Args>
            requires std::constructible_from<Impl, Args...>
        static std::shared_ptr<ServiceInstance> from_direct(Args &&...args);

        template <typename T, std::invocable<T *> Deleter>
        static std::shared_ptr<ServiceInstance> from_unique(std::unique_ptr<T, Deleter> p);

        template <typename T>
        static std::shared_ptr<ServiceInstance> from_shared(std::shared_ptr<T> p);

        template <RefCounted T>
        static std::shared_ptr<ServiceInstance> from_intrusive(RefCountPtr<T> p);

        template <RefCounted T>
        static std::shared_ptr<ServiceInstance> from_intrusive(T *p);

        template <SmartHandle T>
        static std::shared_ptr<ServiceInstance> from_smart_handle(T handle);

        template <typename T>
        static std::shared_ptr<ServiceInstance> from_raw(T *p) noexcept
        {
            return from_raw<T, T>(p);
        }

        template <typename T, std::derived_from<T> Impl>
        static std::shared_ptr<ServiceInstance> from_raw(Impl *p) noexcept;

        template <typename T, typename Deleter>
        static std::shared_ptr<ServiceInstance> from_raw(T *p, Deleter deleter) noexcept
        {
            return from_raw<T, T, Deleter>(p, deleter);
        }

        template <typename T, std::derived_from<T> Impl, typename Deleter>
            requires std::invocable<Deleter, T *>
        static std::shared_ptr<ServiceInstance> from_raw(Impl *p, Deleter deleter) noexcept
        {
            return from_unique<T, Impl, Deleter>(std::unique_ptr<Impl, Deleter>(p, deleter));
        }

      private:
        std::type_index type_ = typeid(void);
        void *ptr_ = nullptr;
    };

    class NonOwningServiceInstance : public ServiceInstance
    {
      public:
        template <typename T>
        explicit inline NonOwningServiceInstance(std::in_place_type_t<T>, T *ptr) noexcept
            : ServiceInstance(typeid(T), ptr)
        {
        }
    };

    template <typename T>
    class DirectServiceInstance final : public ServiceInstance
    {
      public:
        template <typename Base, typename... Args>
            requires std::derived_from<T, Base> && std::constructible_from<T, Args...>
        explicit DirectServiceInstance(std::in_place_type_t<Base>, Args &&...args)
            : ServiceInstance(typeid(T), std::addressof(object_)), object_(std::forward<Args>(args)...)
        {
        }

      private:
        T object_;
    };

    template <typename T, typename Deleter = std::default_delete<T>>
    class UniquePtrServiceInstance : public ServiceInstance
    {
      public:
        explicit UniquePtrServiceInstance(std::unique_ptr<T, Deleter> ptr)
            : ServiceInstance(typeid(T), ptr.get()), ptr_(std::move(ptr))
        {
        }

      private:
        std::unique_ptr<T, Deleter> ptr_;
    };

    template <typename T>
    class SharedPtrServiceInstance : public ServiceInstance
    {
      public:
        explicit SharedPtrServiceInstance(std::shared_ptr<T> ptr)
            : ServiceInstance(typeid(T), ptr.get()), ptr_(std::move(ptr))
        {
        }

      private:
        std::shared_ptr<T> ptr_;
    };

    template <RefCounted T>
    class RefCountedServiceInstance : public ServiceInstance
    {
      public:
        explicit RefCountedServiceInstance(RefCountPtr<T> ptr)
            : ServiceInstance(typeid(T), ptr.get()), ptr_(std::move(ptr))
        {
        }

        explicit RefCountedServiceInstance(T *ptr) : ServiceInstance(typeid(T), ptr), ptr_(ptr)
        {
        }

      private:
        RefCountPtr<T> ptr_;
    };

    template <SmartHandle T>
    class SmartHandleServiceInstance : public ServiceInstance
    {
      public:
        using ElementType = HandleElementType<T>;

        explicit SmartHandleServiceInstance(T handle)
            : ServiceInstance(typeid(ElementType), static_cast<HandleType<ElementType>>(handle.get())),
              handle_(std::move(handle))
        {
        }

      private:
        T handle_;
    };

    template <typename T, std::derived_from<T> Impl>
        requires std::constructible_from<std::remove_cvref_t<Impl>, Impl>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_direct(Impl &&value)
    {
        return std::make_shared<DirectServiceInstance<Impl>>(std::in_place_type<T>, std::forward<Impl>(value));
    }

    template <typename T, std::derived_from<T> Impl, typename... Args>
        requires std::constructible_from<Impl, Args...>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_direct(Args &&...args)
    {
        return std::make_shared<DirectServiceInstance<Impl>>(std::in_place_type<T>, std::forward<Args>(args)...);
    }

    template <typename T, std::invocable<T *> Deleter>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_unique(std::unique_ptr<T, Deleter> p)
    {
        return std::make_shared<UniquePtrServiceInstance<T, Deleter>>(std::move(p));
    }

    template <typename T>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_shared(std::shared_ptr<T> p)
    {
        return std::make_shared<SharedPtrServiceInstance<T>>(std::move(p));
    }

    template <RefCounted T>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_intrusive(RefCountPtr<T> p)
    {
        return std::make_shared<RefCountedServiceInstance<T>>(std::move(p));
    }

    template <RefCounted T>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_intrusive(T *p)
    {
        return std::make_shared<RefCountedServiceInstance<T>>(p);
    }

    template <SmartHandle T>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_smart_handle(T handle)
    {
        return std::make_shared<SmartHandleServiceInstance<T>>(std::move(handle));
    }

    template <typename T, std::derived_from<T> Impl>
    std::shared_ptr<ServiceInstance> ServiceInstance::from_raw(Impl *p) noexcept
    {
        return std::make_shared<NonOwningServiceInstance>(std::in_place_type<T>, p);
    }
} // namespace retro
