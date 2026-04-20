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
    export template <typename T>
    concept RefCounted = requires(T *p) {
        {
            p->retain()
        } noexcept;
        {
            p->release()
        } noexcept;
    };

    export template <typename Block, typename T>
    concept ControlBlockFor = RefCounted<T> && requires(T *p, Block &&b) {
        {
            b.strong_retain(p)
        } noexcept;
        {
            b.strong_release(p)
        } noexcept;
        {
            b.weak_retain()
        } noexcept;
        {
            b.weak_release()
        } noexcept;
        {
            b.strong_ref_count()
        } noexcept -> std::convertible_to<std::uint64_t>;
        {
            b.weak_ref_count()
        } noexcept -> std::convertible_to<std::uint64_t>;
    };

    export template <typename T>
    concept WeakRefCountable = RefCounted<T> && requires(T *p) {
        {
            p->control_block()
        } -> ControlBlockFor<T>;
    };

    export template <RefCounted T>
    class RefCountPtr
    {
      public:
        using element_type = T;

        constexpr RefCountPtr() noexcept = default;

        explicit(false) constexpr RefCountPtr(std::nullptr_t) noexcept
        {
        }

      private:
        explicit constexpr RefCountPtr(T *ptr) noexcept : ptr_(ptr)
        {
        }

      public:
        constexpr RefCountPtr(const RefCountPtr &other) noexcept : ptr_{other.ptr_}
        {
            if (ptr_ != nullptr)
                ptr_->retain();
        }

        template <std::derived_from<T> U>
            requires std::constructible_from<T *, U *>
        explicit(false) constexpr RefCountPtr(const RefCountPtr<U> &other) noexcept : ptr_(other.get())
        {
            if (ptr_ != nullptr)
                ptr_->retain();
        }

        constexpr RefCountPtr(RefCountPtr &&other) noexcept : ptr_(other.ptr_)
        {
            other.ptr_ = nullptr;
        }

        template <std::derived_from<T> U>
            requires std::constructible_from<T *, U *>
        explicit(false) constexpr RefCountPtr(RefCountPtr<U> &&other) noexcept : ptr_(other.release())
        {
            other.ptr_ = nullptr;
        }

        ~RefCountPtr() noexcept
        {
            if (ptr_ != nullptr)
                ptr_->release();
        }

        constexpr RefCountPtr &operator=(const RefCountPtr &other) noexcept
        {
            if (this != std::addressof(other))
            {
                if (other.ptr_ == ptr_)
                    return *this;

                if (ptr_ != nullptr)
                    ptr_->release();

                ptr_ = other.ptr_;
                if (ptr_ != nullptr)
                    ptr_->retain();
            }
            return *this;
        }

        template <std::derived_from<T> U>
            requires std::assignable_from<T *, U *>
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
                    ptr_->release();
                }

                ptr_ = other.ptr_;
                other.ptr_ = nullptr;
            }
            return *this;
        }

        template <std::derived_from<T> U>
            requires std::assignable_from<T *, U *>
        constexpr RefCountPtr &operator=(RefCountPtr<U> &&other) noexcept
        {
            if (ptr_)
            {
                ptr_->release();
            }

            ptr_ = other.ptr_;
            other.ptr_ = nullptr;

            return *this;
        }

        constexpr RefCountPtr &operator=(T *ptr) noexcept
        {
            reset(ptr);
            return *this;
        }

        constexpr static RefCountPtr no_ref(T *ptr) noexcept
        {
            return RefCountPtr{ptr};
        }

        constexpr static RefCountPtr ref(T *ptr) noexcept
        {
            if (ptr != nullptr)
                ptr->retain();
            return RefCountPtr{ptr};
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

        T *release() noexcept
        {
            auto ptr = ptr_;
            ptr_ = nullptr;
            return ptr;
        }

        constexpr void reset() noexcept
        {
            if (ptr_ != nullptr)
            {
                ptr_->release();
                ptr_ = nullptr;
            }
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
        template <RefCounted U>
        friend class RefCountPtr;

        T *ptr_ = nullptr;
    };

    export template <typename T>
        requires RefCounted<std::remove_cv_t<T>>
    constexpr void swap(RefCountPtr<T> &a, RefCountPtr<T> &b) noexcept
    {
        a.swap(b);
    }

    template <WeakRefCountable T>
    using ControlBlockType = std::remove_cvref_t<decltype(std::declval<T>().control_block())>;

    export template <WeakRefCountable T>
    class WeakRefCountPtr
    {
        using ControlBlock = ControlBlockType<T>;

        template <WeakRefCountable U>
        friend class WeakRefCountPtr;

      public:
        constexpr WeakRefCountPtr() noexcept = default;

        explicit(false) WeakRefCountPtr(std::nullptr_t) noexcept
        {
        }

        explicit WeakRefCountPtr(T *ptr) noexcept
            : ptr_{ptr}, control_block_{ptr != nullptr ? std::addressof(ptr->control_block()) : nullptr}
        {
            if (control_block_ != nullptr)
                control_block_->weak_retain();
        }

        WeakRefCountPtr(const WeakRefCountPtr &other) noexcept : ptr_{other.ptr_}, control_block_{other.control_block_}
        {
            if (control_block_ != nullptr)
                control_block_->weak_retain();
        }

        WeakRefCountPtr(WeakRefCountPtr &&other) noexcept : ptr_{other.ptr_}, control_block_{other.control_block_}
        {
            other.ptr_ = nullptr;
            other.control_block_ = nullptr;
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        WeakRefCountPtr(const WeakRefCountPtr &other) noexcept : ptr_{other.ptr_}, control_block_{other.control_block_}
        {
            if (control_block_ != nullptr)
                control_block_->weak_retain();
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        WeakRefCountPtr(WeakRefCountPtr &&other) noexcept : ptr_{other.ptr_}, control_block_{other.control_block_}
        {
            other.ptr_ = nullptr;
            other.control_block_ = nullptr;
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        explicit(false) WeakRefCountPtr(const RefCountPtr<U> &other) noexcept
            : ptr_{other.get()}, control_block_{std::addressof(other->control_block())}
        {
            if (control_block_ != nullptr)
                control_block_->weak_retain();
        }

        ~WeakRefCountPtr() noexcept
        {
            if (control_block_ != nullptr)
                control_block_->weak_release();
        }

        WeakRefCountPtr &operator=(const WeakRefCountPtr &other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            if (control_block_ != nullptr)
                control_block_->weak_release();

            ptr_ = other.ptr_;
            control_block_ = other.control_block_;
            if (control_block_ != nullptr)
                control_block_->weak_retain();

            return *this;
        }

        WeakRefCountPtr &operator=(WeakRefCountPtr &&other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            if (control_block_ != nullptr)
                control_block_->weak_release();

            ptr_ = other.ptr_;
            control_block_ = other.control_block_;
            other.ptr_ = nullptr;
            other.control_block_ = nullptr;
            return *this;
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        WeakRefCountPtr &operator=(const WeakRefCountPtr<U> &other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            if (control_block_ != nullptr)
                control_block_->weak_release();

            ptr_ = other.ptr_;
            control_block_ = other.control_block_;
            if (control_block_ != nullptr)
                control_block_->weak_retain();

            return *this;
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        WeakRefCountPtr &operator=(WeakRefCountPtr<U> &&other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            if (control_block_ != nullptr)
                control_block_->weak_release();

            ptr_ = other.ptr_;
            control_block_ = other.control_block_;
            other.ptr_ = nullptr;
            other.control_block_ = nullptr;
            return *this;
        }

        template <WeakRefCountable U>
            requires std::convertible_to<U *, T *> && std::convertible_to<ControlBlockType<U> *, ControlBlock *>
        WeakRefCountPtr &operator=(const RefCountPtr<U> &other)
        {
            if (control_block_ != nullptr)
                control_block_->weak_release();

            ptr_ = other.get();
            control_block_ = std::addressof(other->control_block());
            if (control_block_ != nullptr)
                control_block_->weak_retain();

            return *this;
        }

        void reset() noexcept
        {
            if (control_block_ == nullptr)
                return;

            control_block_->weak_release();
            ptr_ = nullptr;
            control_block_ = nullptr;
        }

        void swap(WeakRefCountPtr &other) noexcept
        {
            std::swap(ptr_, other.ptr_);
            std::swap(control_block_, other.control_block_);
        }

        [[nodiscard]] std::uint64_t use_count() const noexcept
        {
            return control_block_ != nullptr ? control_block_->strong_ref_count() : 0;
        }

        [[nodiscard]] bool expired() const noexcept
        {
            return use_count() == 0;
        }

        [[nodiscard]] RefCountPtr<T> lock() const noexcept
        {
            return expired() ? RefCountPtr<T>{nullptr} : RefCountPtr<T>::ref(ptr_);
        }

      private:
        T *ptr_ = nullptr;
        ControlBlock *control_block_ = nullptr;
    };

    template <WeakRefCountable T>
    WeakRefCountPtr(T *) -> WeakRefCountPtr<T>;

    template <WeakRefCountable T>
    WeakRefCountPtr(RefCountPtr<T>) -> WeakRefCountPtr<T>;

    export template <RefCounted T, typename... Args>
        requires std::constructible_from<T, Args...>
    constexpr RefCountPtr<T> make_ref_counted(Args &&...args)
    {
        return RefCountPtr<T>::no_ref(new std::remove_cv_t<T>{std::forward<Args>(args)...});
    }

    export template <RefCounted T, RefCounted U>
        requires CanStaticCast<U *, T *>
    constexpr RefCountPtr<T> static_pointer_cast(const RefCountPtr<U> &ptr) noexcept
    {
        auto *p = static_cast<T *>(ptr.get());
        return RefCountPtr<T>::ref(p);
    }

    export template <RefCounted T, RefCounted U>
        requires CanStaticCast<U *, T *>
    constexpr RefCountPtr<U> static_pointer_cast(RefCountPtr<T> &&ptr) noexcept
    {
        auto *p = static_cast<T *>(ptr.release());
        return RefCountPtr<T>::no_ref(p);
    }

    export template <RefCounted T, RefCounted U>
        requires std::derived_from<T, U> || std::derived_from<U, T>
    constexpr RefCountPtr<T> dynamic_pointer_cast(const RefCountPtr<U> &ptr) noexcept
    {
        auto *p = dynamic_cast<T *>(ptr.get());
        return RefCountPtr<T>::ref(p);
    }

    export template <RefCounted T, RefCounted U>
        requires std::derived_from<T, U> || std::derived_from<U, T>
    constexpr RefCountPtr<T> dynamic_pointer_cast(RefCountPtr<U> &&ptr) noexcept
    {
        auto *p = dynamic_cast<T *>(ptr.release());
        return RefCountPtr<T>::no_ref(p);
    }

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

        inline void retain() const noexcept
        {
            ref_count_.fetch_add(1, std::memory_order_relaxed);
        }

        template <typename Self>
        inline void release(this const Self &self) noexcept
        {
            if (static_cast<const IntrusiveRefCounted &>(self).ref_count_.fetch_sub(1, std::memory_order_acq_rel) == 1)
            {
                delete std::addressof(self); // NOSONAR
            }
        }

        [[nodiscard]] inline std::uint32_t ref_count() const noexcept
        {
            return ref_count_.load(std::memory_order_relaxed);
        }

        template <typename Self>
        [[nodiscard]] RefCountPtr<Self> shared_from_this(this const Self &self) noexcept
        {
            return RefCountPtr<Self>::ref(const_cast<Self *>(std::addressof(self)));
        }

      private:
        mutable std::atomic<std::uint32_t> ref_count_{1};
    };

    class RefCountedControlBlock
    {
      public:
        template <RefCounted T>
        void strong_retain([[maybe_unused]] T *ptr) const noexcept
        {
            strong_ref_count_.fetch_add(1, std::memory_order_relaxed);
        }

        template <RefCounted T>
        void strong_release(T *ptr) const noexcept
        {
            if (strong_ref_count_.fetch_sub(1, std::memory_order_relaxed) == 1)
                delete ptr;

            if (weak_ref_count_.load(std::memory_order_relaxed) == 0)
                delete this;
        }

        inline void weak_retain() const noexcept
        {
            weak_ref_count_.fetch_add(1, std::memory_order_relaxed);
        }

        inline void weak_release() const noexcept
        {
            if (const auto old_value = weak_ref_count_.fetch_sub(1, std::memory_order_relaxed);
                old_value == 1 && strong_ref_count_.load(std::memory_order_relaxed) == 0)
                delete this;
        }

        [[nodiscard]] inline std::uint32_t strong_ref_count() const noexcept
        {
            return strong_ref_count_.load(std::memory_order_relaxed);
        }

        [[nodiscard]] inline std::uint32_t weak_ref_count() const noexcept
        {
            return weak_ref_count_.load(std::memory_order_relaxed);
        }

      private:
        mutable std::atomic<std::uint32_t> strong_ref_count_{1};
        mutable std::atomic<std::uint32_t> weak_ref_count_{1};
    };

    export class WeakRefCounted
    {
      protected:
        WeakRefCounted() noexcept = default;

        ~WeakRefCounted() noexcept
        {
            control_block_->weak_release();
        }

      public:
        WeakRefCounted(const WeakRefCounted &) noexcept = delete;
        WeakRefCounted &operator=(const WeakRefCounted &) noexcept = delete;
        WeakRefCounted(WeakRefCounted &&) noexcept = delete;
        WeakRefCounted &operator=(WeakRefCounted &&) noexcept = delete;

        template <typename Self>
        void retain(this const Self &self) noexcept
        {
            self.control_block_->strong_retain(std::addressof(self));
        }

        template <typename Self>
        void release(this const Self &self) noexcept
        {
            self.control_block_->strong_release(std::addressof(self));
        }

        [[nodiscard]] inline std::uint32_t ref_count() const noexcept
        {
            return control_block_->strong_ref_count();
        }

        [[nodiscard]] RefCountedControlBlock &control_block() const noexcept
        {
            return *control_block_;
        }

        template <typename Self>
        [[nodiscard]] RefCountPtr<Self> shared_from_this(this const Self &self) noexcept
        {
            return RefCountPtr<Self>::ref(const_cast<Self *>(std::addressof(self)));
        }

        template <typename Self>
        [[nodiscard]] WeakRefCountPtr<Self> weak_from_this(this const Self &self) noexcept
        {
            return WeakRefCountPtr<Self>{const_cast<Self *>(std::addressof(self))};
        }

      private:
        RefCountedControlBlock *control_block_ = new RefCountedControlBlock{};
    };
} // namespace retro

template <retro::RefCounted T>
struct std::hash<retro::RefCountPtr<T>>
{
    constexpr size_t operator()(const retro::RefCountPtr<T> &ptr) const noexcept
    {
        return hash_value(ptr);
    }
};
