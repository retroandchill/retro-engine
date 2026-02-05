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

namespace retro
{
    export enum class StoragePolicy : std::uint8_t
    {
        External,
        UniqueOwned,
        SharedOwned,
        IntrusiveOwned
    };

    export class ServiceInstance final
    {
      public:
        ServiceInstance() = default;

        ServiceInstance(const ServiceInstance &) = delete;
        inline ServiceInstance(ServiceInstance &&other) noexcept
            : type_{other.type_}, ptr_{other.ptr_}, disposer_(std::move(other.disposer_)),
              shared_ptr_{std::move(other.shared_ptr_)}
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
            disposer_ = std::move(other.disposer_);
            shared_ptr_ = std::move(other.shared_ptr_);

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

        [[nodiscard]] inline bool has_shared_storage() const noexcept
        {
            return shared_ptr_ != nullptr;
        }

        [[nodiscard]] inline const std::shared_ptr<void> &shared_ptr() const noexcept
        {
            return shared_ptr_;
        }

        [[nodiscard]] inline bool is_valid() const noexcept
        {
            return ptr_ != nullptr;
        }

        inline void dispose() noexcept
        {
            if (disposer_)
            {
                auto disposer = std::move(disposer_);
                disposer();
            }

            ptr_ = nullptr;
            type_ = typeid(void);
            shared_ptr_.reset();
        }

        template <typename T>
        static ServiceInstance from_unique(std::unique_ptr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.disposer_ = [p = std::move(p)]() mutable noexcept
            {
                p.reset();
            };
            return instance;
        }

        template <typename T>
        static ServiceInstance from_shared(std::shared_ptr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.shared_ptr_ = p;
            instance.disposer_ = {};
            return instance;
        }

        template <RefCounted T>
        static ServiceInstance from_intrusive(RefCountPtr<T> p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p.get();
            instance.disposer_ = [p = std::move(p)]() mutable noexcept
            {
                p.reset();
            };
            return instance;
        }

        template <RefCounted T>
        static ServiceInstance from_intrusive(T *p)
        {
            ServiceInstance instance;
            instance.type_ = typeid(T);
            instance.ptr_ = p;
            instance.disposer_ = [p = RefCountPtr<T>(p)]() mutable noexcept
            {
                p.reset();
            };
            return instance;
        }

        template <typename T, typename Deleter>
        static ServiceInstance from_raw(T *p, Deleter deleter) noexcept
        {
            ServiceInstance inst;
            inst.type_ = typeid(T);
            inst.ptr_ = p;

            inst.disposer_ = [p, deleter]() noexcept
            {
                if constexpr (std::is_pointer_v<Deleter>)
                {
                    if (p != nullptr && deleter != nullptr)
                    {
                        deleter(p);
                    }
                }
                else
                {
                    if (p != nullptr)
                    {
                        deleter(p);
                    }
                }
            };
            return inst;
        }

      private:
        std::move_only_function<void()> disposer_{};
        std::type_index type_ = typeid(void);
        void *ptr_ = nullptr;
        std::shared_ptr<void> shared_ptr_{};
    };
} // namespace retro
