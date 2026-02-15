/**
 * @file ref_counted_ptr.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.memory.ref_counted_ptr;

import retro.core.type_traits.basic;
import std;

namespace retro
{
    template <typename T>
    concept SelfRetainable = requires(T *p) {
        {
            p->retain()
        } noexcept;
    };

    template <typename T>
    concept SelfReleaseable = requires(T *p) {
        {
            p->release()
        } noexcept;
    };

    template <SelfRetainable T>
    void intrusive_retain(T *p) noexcept
    {
        p->retain();
    }

    template <SelfReleaseable T>
    void intrusive_release(T *p) noexcept
    {
        p->release();
    }

    export template <typename T>
    concept RefCounted = requires(T *p) {
        {
            intrusive_retain(p)
        } noexcept;
        {
            intrusive_release(p)
        } noexcept;
    };

    export class IntrusiveRefCounted
    {
      protected:
        IntrusiveRefCounted() noexcept = default;
        ~IntrusiveRefCounted() noexcept = default;

      public:
        IntrusiveRefCounted(const IntrusiveRefCounted &) noexcept = delete;
        IntrusiveRefCounted &operator=(const IntrusiveRefCounted &) noexcept = delete;
        IntrusiveRefCounted(IntrusiveRefCounted &&) noexcept = delete;
        IntrusiveRefCounted &operator=(IntrusiveRefCounted &&) noexcept = delete;

        inline void retain() noexcept
        {
            ref_count_.fetch_add(1, std::memory_order_relaxed);
        }

        template <typename Self>
        inline void release(this Self &self) noexcept
        {
            if (static_cast<IntrusiveRefCounted &>(self).ref_count_.fetch_sub(1, std::memory_order_acq_rel) == 1)
            {
                delete std::addressof(self); // NOSONAR
            }
        }

        [[nodiscard]] inline std::uint32_t ref_count() const noexcept
        {
            return ref_count_.load(std::memory_order_relaxed);
        }

      private:
        std::atomic<std::uint32_t> ref_count_{0};
    };

    struct RefCountInternal
    {
    };

    constexpr RefCountInternal ref_count_internal{};

    export template <typename T>
        requires RefCounted<std::remove_cv_t<T>>
    class RefCountPtr
    {
        using NonConstType = std::remove_cv_t<T>;

      public:
        using element_type = T;

        constexpr RefCountPtr() noexcept = default;

        explicit(false) constexpr RefCountPtr(std::nullptr_t) noexcept
        {
        }

        explicit constexpr RefCountPtr(NonConstType *ptr) noexcept : ptr_(ptr)
        {
            if (ptr_ != nullptr)
                intrusive_retain(ptr_);
        }

        explicit constexpr RefCountPtr(RefCountInternal, NonConstType *ptr) noexcept : ptr_(ptr)
        {
        }

        constexpr RefCountPtr(const RefCountPtr &other) noexcept : ptr_(other.ptr_)
        {
            if (ptr_ != nullptr)
                intrusive_retain(ptr_);
        }

        template <std::derived_from<T> U>
            requires std::constructible_from<NonConstType *, U *>
        explicit(false) constexpr RefCountPtr(const RefCountPtr<U> &other) noexcept : ptr_(other.get())
        {
            if (ptr_ != nullptr)
                intrusive_retain(ptr_);
        }

        constexpr RefCountPtr(RefCountPtr &&other) noexcept : ptr_(other.ptr_)
        {
            other.ptr_ = nullptr;
        }

        template <std::derived_from<T> U>
            requires std::constructible_from<NonConstType *, U *>
        explicit(false) constexpr RefCountPtr(RefCountPtr<U> &&other) noexcept : ptr_(other.release(RefCountInternal{}))
        {
            other.ptr_ = nullptr;
        }

        ~RefCountPtr() noexcept
        {
            if (ptr_ != nullptr)
                intrusive_release(ptr_);
        }

        constexpr RefCountPtr &operator=(const RefCountPtr &other) noexcept
        {
            if (this != std::addressof(other))
            {
                reset(other.ptr_);
            }
            return *this;
        }

        template <std::derived_from<T> U>
            requires std::assignable_from<NonConstType *, U *>
        constexpr RefCountPtr &operator=(const RefCountPtr<U> &other) noexcept
        {
            reset(other.get());
            return *this;
        }

        constexpr RefCountPtr &operator=(RefCountPtr &&other) noexcept
        {
            if (this != std::addressof(other))
            {
                if (ptr_)
                {
                    intrusive_release(ptr_);
                }

                ptr_ = other.ptr_;
                other.ptr_ = nullptr;
            }
            return *this;
        }

        template <std::derived_from<T> U>
            requires std::assignable_from<NonConstType *, U *>
        constexpr RefCountPtr &operator=(RefCountPtr<U> &&other) noexcept
        {
            if (ptr_)
            {
                intrusive_release(ptr_);
            }

            ptr_ = other.ptr_;
            other.ptr_ = nullptr;

            return *this;
        }

        constexpr RefCountPtr &operator=(NonConstType **ptr) noexcept
        {
            reset(ptr);
            return *this;
        }

        [[nodiscard]] constexpr T *get() const noexcept
        {
            return ptr_;
        }

        [[nodiscard]] constexpr T *operator->() const noexcept
        {
            return ptr_;
        }

        [[nodiscard]] constexpr T &operator*() const noexcept
        {
            return *ptr_;
        }

        NonConstType *release(RefCountInternal) noexcept
        {
            auto ptr = ptr_;
            ptr_ = nullptr;
            return ptr;
        }

        [[nodiscard]] constexpr void reset() noexcept
        {
            if (ptr_ != nullptr)
            {
                intrusive_release(ptr_);
                ptr_ = nullptr;
            }
        }

        constexpr void reset(T *ptr) noexcept
        {
            if (ptr == ptr_)
                return;

            if (ptr_ != nullptr)
                intrusive_release(ptr_);

            ptr_ = ptr;
            if (ptr_ != nullptr)
                intrusive_retain(ptr_);
        }

        constexpr void swap(RefCountPtr &other) noexcept
        {
            std::swap(ptr_, other.ptr_);
        }

        [[nodiscard]] friend constexpr bool operator==(const RefCountPtr &lhs, const RefCountPtr &rhs) noexcept
        {
            return lhs.ptr_ == rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend constexpr bool operator==(const RefCountPtr &lhs, const RefCountPtr<U> &rhs) noexcept
        {
            return lhs.ptr_ == rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend constexpr bool operator==(const RefCountPtr<U> &lhs, const RefCountPtr &rhs) noexcept
        {
            return lhs.ptr_ == rhs.ptr_;
        }

        [[nodiscard]] friend constexpr bool operator==(const RefCountPtr &lhs, std::nullptr_t) noexcept
        {
            return lhs.ptr_ == nullptr;
        }

        [[nodiscard]] friend constexpr bool operator==(std::nullptr_t, const RefCountPtr &rhs) noexcept
        {
            return rhs.ptr_ == nullptr;
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const RefCountPtr &lhs,
                                                                        const RefCountPtr &rhs) noexcept
        {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const RefCountPtr &lhs, std::nullptr_t) noexcept
        {
            return lhs.ptr_ <=> nullptr;
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(std::nullptr_t, const RefCountPtr &rhs) noexcept
        {
            return nullptr <=> rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const RefCountPtr &lhs,
                                                                        const RefCountPtr<U> &rhs) noexcept
        {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const RefCountPtr<U> &lhs,
                                                                        const RefCountPtr &rhs) noexcept
        {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        template <Char CharType, typename Traits>
        friend std::basic_ostream<CharType, Traits> &operator<<(std::basic_ostream<CharType, Traits> &stream,
                                                                const RefCountPtr<T> &ptr)
        {
            return stream << ptr.get();
        }

        friend constexpr std::size_t hash_value(const RefCountPtr path)
        {
            return std::hash<T *>{}(path.ptr_);
        }

      private:
        template <typename U>
            requires RefCounted<std::remove_cv_t<U>>
        friend class RefCountPtr;

        NonConstType *ptr_ = nullptr;
    };

    export template <RefCounted T>
    RefCountPtr(T *) -> RefCountPtr<T>;

    export template <typename T>
        requires RefCounted<std::remove_cv_t<T>>
    constexpr void swap(RefCountPtr<T> &a, RefCountPtr<T> &b) noexcept
    {
        a.swap(b);
    }

    export template <RefCounted T, typename... Args>
        requires std::constructible_from<T, Args...>
    constexpr RefCountPtr<T> make_ref_counted(Args &&...args)
    {
        return RefCountPtr<T>{new std::remove_cv_t<T>{std::forward<Args>(args)...}};
    }

    export template <RefCounted T, RefCounted U>
        requires CanStaticCast<T *, U *>
    constexpr RefCountPtr<U> static_pointer_cast(const RefCountPtr<T> &ptr) noexcept
    {
        auto *p = static_cast<U *>(ptr.get());
        return RefCountPtr<U>{p};
    }

    export template <RefCounted T, RefCounted U>
        requires CanStaticCast<T *, U *>
    constexpr RefCountPtr<U> static_pointer_cast(RefCountPtr<T> &&ptr) noexcept
    {
        auto *p = static_cast<U *>(ptr.release(ref_count_internal));
        return RefCountPtr<U>{ref_count_internal, p};
    }

    export template <RefCounted T, RefCounted U>
        requires std::derived_from<T, U> || std::derived_from<U, T>
    constexpr RefCountPtr<U> dynamic_pointer_cast(const RefCountPtr<T> &ptr) noexcept
    {
        auto *p = dynamic_cast<U *>(ptr.get());
        return RefCountPtr<U>{p};
    }

    export template <RefCounted T, RefCounted U>
        requires std::derived_from<T, U> || std::derived_from<U, T>
    constexpr RefCountPtr<U> dynamic_pointer_cast(RefCountPtr<T> &&ptr) noexcept
    {
        auto *p = dynamic_cast<U *>(ptr.release(ref_count_internal));
        return RefCountPtr<U>{ref_count_internal, p};
    }
} // namespace retro

template <retro::RefCounted T>
struct std::hash<retro::RefCountPtr<T>>
{
    constexpr size_t operator()(const retro::RefCountPtr<T> &ptr) const noexcept
    {
        return hash_value(ptr);
    }
};
