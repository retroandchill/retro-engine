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
import retro.core.memory.small_unique_ptr;
import :metadata;

namespace retro
{
    export enum class StoragePolicy : std::uint8_t
    {
        External,
        Direct,
        UniqueOwned,
        SharedOwned,
        IntrusiveOwned
    };

    export class ServiceInstance final
    {
        static constexpr std::size_t storage_size = 24;

      public:
        ServiceInstance() = default;

        ServiceInstance(const ServiceInstance &) = delete;
        inline ServiceInstance(ServiceInstance &&other) noexcept
            : type_{other.type_}, ptr_{other.ptr_}, storage_(std::move(other.storage_))
        {
            other.type_ = typeid(void);
            other.ptr_ = nullptr;
        }

        inline ~ServiceInstance() noexcept
        {
            dispose();
        }

        ServiceInstance &operator=(const ServiceInstance &) = delete;
        inline ServiceInstance &operator=(ServiceInstance &&other) noexcept
        {
            if (this == &other)
            {
                return *this;
            }

            dispose();

            type_ = other.type_;
            ptr_ = other.ptr_;
            storage_ = std::move(other.storage_);

            other.type_ = typeid(void);
            other.ptr_ = nullptr;

            return *this;
        }

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

        inline void dispose() noexcept
        {
            storage_.reset();
            ptr_ = nullptr;
            type_ = typeid(void);
        }

        template <typename T>
            requires std::constructible_from<std::remove_cvref_t<T>, T>
        static ServiceInstance from_direct(T &&value)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.storage_ =
                make_unique_small<StorageImpl<std::remove_cvref_t<T>>, storage_size>(std::forward<T>(value));
            instance.ptr_ = instance.storage_->get_raw_storage();
            return instance;
        }

        template <typename T>
        static ServiceInstance from_unique(std::unique_ptr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.storage_ = make_unique_small<StorageImpl<std::unique_ptr<T>>, storage_size>(std::move(p));
            return instance;
        }

        template <typename T>
        static ServiceInstance from_shared(std::shared_ptr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.storage_ = make_unique_small<StorageImpl<std::shared_ptr<T>>, storage_size>(std::move(p));
            return instance;
        }

        template <RefCounted T>
        static ServiceInstance from_intrusive(RefCountPtr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.storage_ = make_unique_small<StorageImpl<RefCountPtr<T>>, storage_size>(std::move(p));
            return instance;
        }

        template <RefCounted T>
        static ServiceInstance from_intrusive(T *p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p;
            instance.storage_ = make_unique_small<StorageImpl<RefCountPtr<T>>, storage_size>(RefCountPtr<T>{p});
            return instance;
        }

        template <SmartHandle T>
        static ServiceInstance from_smart_handle(T handle)
        {
            ServiceInstance instance;
            using ElementType = HandleElementType<T>;
            instance.type_ = typeid(ElementType);
            instance.ptr_ = static_cast<HandleType<ElementType>>(handle.get());
            instance.storage_ = make_unique_small<StorageImpl<T>, storage_size>(std::move(handle));
            return instance;
        }

        template <typename T>
        static ServiceInstance from_raw(T *p) noexcept
        {
            ServiceInstance inst;
            inst.type_ = typeid(T);
            inst.ptr_ = p;
            return inst;
        }

        template <typename T, typename Deleter>
            requires std::invocable<Deleter, T *>
        static ServiceInstance from_raw(T *p, Deleter deleter) noexcept
        {
            ServiceInstance inst;
            inst.type_ = typeid(T);
            inst.ptr_ = p;
            inst.storage_ = make_unique_small<StorageImpl<std::unique_ptr<T, Deleter>>, storage_size>(
                std::unique_ptr<T, Deleter>{p, std::move(deleter)});
            return inst;
        }

      private:
        class Storage
        {
          public:
            virtual ~Storage() = default;

            [[nodiscard]] virtual void *get_raw_storage() noexcept = 0;

            virtual void small_unique_ptr_move(void *dst) noexcept = 0;
        };

        template <typename T>
        class StorageImpl final : public Storage
        {
          public:
            explicit StorageImpl(T value) : value_{std::move(value)}
            {
            }

            [[nodiscard]] void *get_raw_storage() noexcept override
            {
                return std::addressof(value_);
            }

            void small_unique_ptr_move(void *dst) noexcept override
            {
                std::construct_at(static_cast<StorageImpl *>(dst), std::move(*this));
            }

          private:
            T value_;
        };

        SmallUniquePtr<Storage, storage_size> storage_;
        std::type_index type_ = typeid(void);
        void *ptr_ = nullptr;
    };
} // namespace retro
