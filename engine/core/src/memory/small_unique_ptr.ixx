/**
 * @file small_unique_ptr.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

export module retro.core.memory.small_unique_ptr;

import std;

namespace retro
{
    template <std::integral T>
    constexpr T max_pow2_factor(T n) noexcept
    {
        return n & (~n + 1);
    }

    struct IgnoreTag
    {
        constexpr const IgnoreTag &operator=(const auto &) const noexcept
        {
            return *this;
        }
    };

    using MoveFn = void (*)(void *src, void *dest) noexcept;

    template <typename T>
        requires(!std::is_const_v<T> && !std::is_volatile_v<T> && !std::is_array_v<T>)
    void move_buffer(void *src, void *dest) noexcept(std::is_nothrow_move_constructible_v<T>)
    {
        std::construct_at(static_cast<T *>(dest), std::move(*static_cast<T *>(src)));
    }

    template <typename T>
    concept HasVirtualMove = requires(std::remove_cv_t<T> &object, void *const dst) {
        requires std::is_polymorphic_v<T>;
        {
            object.small_unique_ptr_move(dst)
        } noexcept -> std::same_as<void>;
    };

    template <typename T>
    concept NothrowDereferenceable = noexcept(*std::declval<T>());

    template <typename T, typename U>
    concept SameUnqualifed = std::same_as<std::remove_cv_t<T>, std::remove_cv_t<U>>;

    template <typename T>
    struct CvQualRank : std::integral_constant<std::size_t, std::is_const_v<T> + std::is_volatile_v<T>>
    {
    };

    export template <typename T>
    constexpr std::size_t cv_qual_rank = CvQualRank<T>::value;

    export template <typename T, typename U>
    concept LessCvQualified = cv_qual_rank<T> < cv_qual_rank<U>;

    export template <typename Base, typename Derived>
    concept ProperBaseOf =
        std::derived_from<std::remove_cv_t<Derived>, std::remove_cv_t<Base>> && !SameUnqualifed<Base, Derived>;

    export template <typename From, typename To>
    concept PointerConvertible =
        (ProperBaseOf<To, From> && std::has_virtual_destructor_v<To> && !LessCvQualified<To, From>) ||
        (SameUnqualifed<From, To> && !LessCvQualified<To, From>);

    template <typename T, std::size_t SmallPtrSize>
    struct BufferSize
    {
      private:
        static constexpr std::size_t dynamic_buffer_size =
            SmallPtrSize - sizeof(T *) - !HasVirtualMove<T> * sizeof(MoveFn);
        static constexpr std::size_t static_buffer_size = std::min(sizeof(T), SmallPtrSize - sizeof(T *));

      public:
        static constexpr std::size_t value =
            std::has_virtual_destructor_v<T> ? dynamic_buffer_size : static_buffer_size;
    };

    template <typename T>
    struct BufferSize<T, sizeof(void *)>
    {
        static constexpr std::size_t value = 0;
    };

    template <typename T, std::size_t SmallPtrSize>
    struct BufferSize<T[], SmallPtrSize>
    {
        static constexpr std::size_t value = SmallPtrSize - sizeof(T *);
    };

    template <typename T>
    struct BufferSize<T[], sizeof(void *)>
    {
        static constexpr std::size_t value = 0;
    };

    template <typename T, std::size_t SmallPtrSize>
    constexpr std::size_t buffer_size = BufferSize<T, SmallPtrSize>::value;

    template <typename T, std::size_t SmallPtrSize>
    struct BufferAlignment
    {
      private:
        static constexpr std::size_t dynamic_buffer_alignment = max_pow2_factor(SmallPtrSize);
        static constexpr std::size_t static_buffer_alignment = max_pow2_factor(std::min(alignof(T), SmallPtrSize));

      public:
        static constexpr std::size_t value =
            std::has_virtual_destructor_v<T> ? dynamic_buffer_alignment : static_buffer_alignment;
    };

    template <typename T, std::size_t SmallPtrSize>
    constexpr std::size_t buffer_alignment = BufferAlignment<T, SmallPtrSize>::value;

    template <typename T>
    using Simplified = std::remove_cv_t<std::remove_extent_t<T>>;

    export template <typename T, std::size_t SmallPtrSize>
    concept AlwaysHeapAllocated =
        (sizeof(Simplified<T>) > buffer_size<T, SmallPtrSize>) ||
        (alignof(Simplified<T>) > buffer_alignment<T, SmallPtrSize>) ||
        (!std::is_nothrow_move_constructible_v<Simplified<T>> && !std::is_abstract_v<Simplified<T>>);

    template <typename T, std::size_t Size>
    struct SmallUniquePtrBase
    {
        using pointer = std::remove_cv_t<T> *;
        using buffer_t = std::byte[buffer_size<T, Size>];

        pointer buffer(const std::ptrdiff_t offset = 0) const noexcept
        {
            assert(offset <= sizeof(buffer_t));
            return reinterpret_cast<pointer>(static_cast<std::byte *>(buffer_) + offset);
        }

        template <typename U>
        void move_buffer_to(SmallUniquePtrBase<U, Size> &dest) noexcept
        {
            move_(std::launder(buffer()), dest.buffer());
            dest.move_ = move_;
        }

        constexpr bool is_stack_allocated() const noexcept
        {
            return move_ != nullptr;
        }

        constexpr SmallUniquePtrBase() noexcept
        {
            if consteval
            {
                std::fill_n(buffer_, sizeof(buffer_), std::byte{0});
            }
        }

        alignas(buffer_alignment<T, Size>) mutable buffer_t buffer_;
        T *data_;
        MoveFn move_;
    };

    template <typename T, std::size_t Size>
        requires AlwaysHeapAllocated<T, Size>
    struct SmallUniquePtrBase<T, Size>
    {
        static constexpr bool is_stack_allocated() noexcept
        {
            return false;
        }

        std::remove_extent_t<T> *data_;
        static constexpr IgnoreTag move_;
    };

    template <typename T, std::size_t Size>
        requires(!AlwaysHeapAllocated<T, Size> && !std::is_polymorphic_v<T> && !std::is_array_v<T>)
    struct SmallUniquePtrBase<T, Size>
    {
        using pointer = std::remove_cv_t<T> *;
        using buffer_t = std::remove_cv_t<T>;

        constexpr pointer buffer(std::ptrdiff_t offset = 0) const noexcept
        {
            return std::addressof(buffer_);
        }

        template <typename U>
        constexpr void move_buffer_to(SmallUniquePtrBase<U, Size> &dest) noexcept
        {
            std::construct_at(dest.buffer(), std::move(*buffer()));
        }

        constexpr bool is_stack_allocated() const noexcept
        {
            return !std::is_constant_evaluated() && data_ != nullptr;
        }

        // NOLINTNEXTLINE
        constexpr SmallUniquePtrBase() noexcept
        {
        }

        SmallUniquePtrBase(const SmallUniquePtrBase &) = default;
        SmallUniquePtrBase(SmallUniquePtrBase &&) noexcept = default;

        constexpr ~SmallUniquePtrBase() noexcept
        {
        }

        SmallUniquePtrBase &operator=(const SmallUniquePtrBase &) = default;
        SmallUniquePtrBase &operator=(SmallUniquePtrBase &&) noexcept = default;

        union
        {
            mutable buffer_t buffer_;
        };
        T *data_;
        static constexpr IgnoreTag move_;
    };

    template <typename T, std::size_t Size>
        requires(!AlwaysHeapAllocated<T, Size> && HasVirtualMove<T>)
    struct SmallUniquePtrBase<T, Size>
    {
        using pointer = std::remove_cv_t<T> *;
        using buffer_t = std::byte[buffer_size<T, Size>];

        pointer buffer(std::ptrdiff_t offset = 0) const noexcept
        {
            assert(offset <= sizeof(buffer_t));
            return reinterpret_cast<pointer>(static_cast<std::byte *>(buffer_) + offset);
        }

        template <HasVirtualMove U>
        void move_buffer_to(SmallUniquePtrBase<U, Size> &dest) noexcept
        {
            const pointer data = const_cast<pointer>(data_);
            data->small_unique_ptr_move(dest.buffer());
        }

        constexpr bool is_stack_allocated() const noexcept
        {
            if consteval
            {
                return false;
            }

            auto *data = reinterpret_cast<const volatile std::byte *>(data_);
            auto *buffer_first = static_cast<const volatile std::byte *>(buffer_);
            auto *buffer_last = buffer_first + buffer_size<T, Size>;

            assert(reinterpret_cast<std::uintptr_t>(buffer_last) - reinterpret_cast<std::uintptr_t>(buffer_first) ==
                   (buffer_size<T, Size>)&&"Linear address space assumed for the stack buffer");

            return std::less_equal{}(buffer_first, data) && std::less{}(data, buffer_last);
        }

        constexpr SmallUniquePtrBase() noexcept
        {
            if consteval
            {
                std::fill_n(buffer_, sizeof(buffer_), std::byte{0});
            }
        }

        alignas(buffer_alignment<T, Size>) mutable buffer_t buffer_;
        T *data_;
        static constexpr IgnoreTag move_;
    };

    template <typename T, std::size_t Size>
        requires(!AlwaysHeapAllocated<T, Size> && std::is_array_v<T>)
    struct SmallUniquePtrBase<T, Size>
    {
        static constexpr std::size_t array_size = buffer_size<T, Size> / sizeof(std::remove_extent_t<T>);

        using pointer = Simplified<T> *;
        using buffer_t = Simplified<T>[array_size];

        constexpr pointer buffer(std::ptrdiff_t = 0) const noexcept
        {
            return static_cast<pointer>(buffer_);
        }

        template <typename U>
        void move_buffer_to(SmallUniquePtrBase<U, Size> &dest) noexcept
        {
            std::uninitialized_move(buffer(), buffer() + array_size, dest.buffer());
        }

        constexpr bool is_stack_allocated() const noexcept
        {
            return !std::is_constant_evaluated() && (data_ == buffer());
        }

        // NOLINTNEXTLINE
        constexpr SmallUniquePtrBase() noexcept
        {
        }

        SmallUniquePtrBase(const SmallUniquePtrBase &) = default;
        SmallUniquePtrBase(SmallUniquePtrBase &&) noexcept = default;

        constexpr ~SmallUniquePtrBase() noexcept
        {
        }

        SmallUniquePtrBase &operator=(const SmallUniquePtrBase &) = default;
        SmallUniquePtrBase &operator=(SmallUniquePtrBase &&) noexcept = default;

        union
        {
            mutable buffer_t buffer_;
        };
        std::remove_extent_t<T> *data_;
        static constexpr IgnoreTag move_;
    };

    export constexpr std::size_t default_small_ptr_size = 64;

    export template <typename T, std::size_t Size = default_small_ptr_size>
        requires(!std::is_bounded_array_v<T> && (Size >= sizeof(T *)) && (Size % sizeof(T *) == 0))
    class SmallUniquePtr : private SmallUniquePtrBase<T, Size>
    {
      public:
        using element_type = std::remove_extent_t<T>;
        using pointer = std::remove_extent_t<T> *;
        using reference = std::remove_extent_t<T> &;

        struct ConstructorTag
        {
        };

        constexpr SmallUniquePtr() noexcept
        {
            this->data_ = nullptr;
            this->move_ = nullptr;
        }

        constexpr explicit(false) SmallUniquePtr(std::nullptr_t) noexcept : SmallUniquePtr()
        {
        }

        SmallUniquePtr(const SmallUniquePtr &) = delete;

        constexpr SmallUniquePtr(SmallUniquePtr &&other) noexcept : SmallUniquePtr(std::move(other), ConstructorTag{})
        {
        }

        template <PointerConvertible<T> U>
        constexpr explicit(false) SmallUniquePtr(SmallUniquePtr<U, Size> &&other) noexcept
            : SmallUniquePtr(std::move(other), ConstructorTag{})
        {
        }

        template <PointerConvertible<T> U>
        constexpr SmallUniquePtr(SmallUniquePtr<U, Size> &&other, ConstructorTag) noexcept
        {
            if (!other.is_stack_allocated())
            {
                this->data_ = other.data_;
                this->move_ = nullptr;
                other.data_ = nullptr;
                return;
            }

            if constexpr (!AlwaysHeapAllocated<U, Size>)
            {
                const std::ptrdiff_t base_offset = other.template offsetof_base<T>();
                const pointer base_pointer = this->buffer(base_offset);
                other.move_buffer_to(*this);
                this->data_ = std::launder(base_pointer);
            }
        }

        constexpr ~SmallUniquePtr() noexcept
        {
            destroy();
        }

        SmallUniquePtr &operator=(const SmallUniquePtr &) = delete;

        constexpr SmallUniquePtr &operator=(SmallUniquePtr &&other) noexcept
        {
            if (std::addressof(other) == this)
                return *this;

            operator= <T>(std::move(other));
            return *this;
        }

        template <PointerConvertible<T> U>
        constexpr SmallUniquePtr &operator=(SmallUniquePtr<U, Size> &&other) noexcept
        {
            if (!other.is_stack_allocated())
            {
                reset(std::exchange(other.data_, nullptr));
                return *this;
            }

            if constexpr (!AlwaysHeapAllocated<U, Size>)
            {
                destroy();
                const std::ptrdiff_t base_offset = other.template offsetof_base<T>();
                const pointer base_pointer = this->buffer(base_offset);
                other.move_buffer_to(*this);
                this->data_ = std::launder(base_pointer);
            }

            return *this;
        }

        constexpr SmallUniquePtr &operator=(std::nullptr_t) noexcept
        {
            reset();
            return *this;
        }

        constexpr void reset(pointer new_data = pointer{}) noexcept
        {
            is_stack_allocated() ? reset_internal(new_data) : reset_allocated(new_data);
        }

        constexpr void swap(SmallUniquePtr &other) noexcept
        {
            if constexpr (SmallUniquePtr::is_always_heap_allocated())
            {
                std::swap(this->data_, other.data_);
            }
            else if (!is_stack_allocated() && !other.is_stack_allocated())
            {
                std::swap(this->data_, other.data_);
            }
            else if (is_stack_allocated() && other.is_stack_allocated())
            {
                const auto offset_other = other.template offsetof_base<>();

                SmallUniquePtrBase<T, Size> temp;
                if constexpr (HasVirtualMove<T>)
                {
                    temp.data_ = std::launder(temp.buffer(offset_other));
                }
                other.move_buffer_to(temp);

                other.destroy_buffer();
                other.data_ = std::launder(other.buffer(this->offsetof_base()));
                this->move_buffer_to(other);

                this->destroy_buffer();
                temp.move_buffer_to(*this);
                this->data_ = std::launder(this->buffer(offset_other));
            }
            else if (!is_stack_allocated())
            {
                const pointer new_data = this->buffer(other.offsetof_base());
                other.move_buffer_to(*this);
                other.reset_internal(this->data_);
                this->data_ = std::launder(new_data);
            }
            else
            {
                const pointer new_data = other.buffer(this->offsetof_base());
                this->move_buffer_to(other);
                this->reset_internal(other.data_);
                other.data_ = std::launder(new_data);
            }
        }

        [[nodiscard]] pointer release() noexcept = delete;

        [[nodiscard]] constexpr bool is_stack_allocated() const noexcept
        {
            return SmallUniquePtr::SmallUniquePtrBase::is_stack_allocated();
        }

        [[nodiscard]] static constexpr bool is_always_heap_allocated() noexcept
        {
            return AlwaysHeapAllocated<T, Size>;
        }

        [[nodiscard]] static constexpr std::size_t stack_buffer_size() noexcept
        {
            if constexpr (is_always_heap_allocated())
            {
                return 0;
            }
            else
            {
                return sizeof(SmallUniquePtr::buffer_t);
            }
        }

        [[nodiscard]] static constexpr std::size_t stack_array_size() noexcept
            requires(std::is_array_v<T>)
        {
            return stack_buffer_size() / sizeof(std::remove_extent_t<T>);
        }

        [[nodiscard]] constexpr pointer get() const noexcept
        {
            return this->data_;
        }

        [[nodiscard]] constexpr explicit operator bool() const noexcept
        {
            return static_cast<bool>(this->data_);
        }

        [[nodiscard]] constexpr reference operator*() const noexcept(NothrowDereferenceable<pointer>)
            requires(!std::is_array_v<T>)
        {
            assert(this->data_ != nullptr);
            return *this->data_;
        }

        [[nodiscard]] constexpr pointer operator->() const noexcept
            requires(!std::is_array_v<T>)
        {
            assert(this->data_ != nullptr);
            return this->data_;
        }

        [[nodiscard]] constexpr reference operator[](std::size_t index) const noexcept
            requires(std::is_array_v<T>)
        {
            assert(this->data_ != nullptr);
            return this->data_[index];
        }

        constexpr bool operator==(std::nullptr_t) const noexcept
        {
            return this->data_ == pointer{nullptr};
        }

        constexpr std::strong_ordering operator<=>(std::nullptr_t) const noexcept
        {
            return std::compare_three_way{}(this->data_, pointer{nullptr});
        }

        template <typename U>
        constexpr bool operator==(const SmallUniquePtr<U, Size> &other) const noexcept
        {
            return this->data_ == other.data_;
        }

        template <typename U>
        constexpr std::strong_ordering operator<=>(const SmallUniquePtr<U, Size> &other) const noexcept
        {
            return std::compare_three_way{}(this->data_, other.data_);
        }

        template <typename CharT, typename Traits>
        friend std::basic_ostream<CharT, Traits> &operator<<(std::basic_ostream<CharT, Traits> &os,
                                                             const SmallUniquePtr &ptr)
        {
            return os << ptr.get();
        }

        constexpr friend void swap(SmallUniquePtr &lhs, SmallUniquePtr &rhs) noexcept
        {
            lhs.swap(rhs);
        }

      private:
        template <typename Base = T>
        [[nodiscard]] constexpr std::ptrdiff_t offsetof_base() const noexcept
        {
            if constexpr (std::is_polymorphic_v<T>)
            {
                assert(is_stack_allocated());

                auto *derived_ptr = reinterpret_cast<const volatile std::byte *>(this->buffer());
                auto *base_ptr =
                    reinterpret_cast<const volatile std::byte *>(static_cast<const volatile Base *>(this->data_));

                return base_ptr - derived_ptr;
            }
            else
            {
                return 0;
            }
        }

        constexpr void destroy_buffer() noexcept
        {
            assert(is_stack_allocated());
            if constexpr (std::is_array_v<T>)
            {
                std::destroy(this->data_, this->data_ + stack_array_size());
            }
            else
            {
                std::destroy_at(this->data_);
            }
        }

        constexpr void destroy_allocated() noexcept
        {
            assert(!is_stack_allocated());
            if constexpr (std::is_array_v<T>)
            {
                delete[] this->data_;
            }
            else
            {
                delete this->data_;
            }
        }

        constexpr void destroy() noexcept
        {
            is_stack_allocated() ? destroy_buffer() : destroy_allocated();
        }

        constexpr void reset_internal(pointer new_data) noexcept
        {
            assert(is_stack_allocated());

            destroy_buffer();
            this->data_ = new_data;
            this->move_ = nullptr;
        }

        constexpr void reset_allocated(pointer new_data) noexcept
        {
            assert(!is_stack_allocated());

            destroy_allocated();
            this->data_ = new_data;
        }

        template <typename U, std::size_t S>
            requires(!std::is_bounded_array_v<U> && (S >= sizeof(U *)) && (S % sizeof(U *) == 0))
        friend class SmallUniquePtr;

        friend struct MakeUniqueSmallImpl;
    };

    struct MakeUniqueSmallImpl
    {
        template <typename T, typename B, std::size_t S, typename... Args>
            requires std::constructible_from<T, Args...>
        static constexpr SmallUniquePtr<B, S> invoke_scalar(Args &&...args) noexcept(
            std::is_nothrow_constructible_v<T, Args...> && !AlwaysHeapAllocated<T, S>)
        {
            SmallUniquePtr<B, S> ptr;

            if consteval
            {
                ptr.data_ = new T(std::forward<Args>(args)...);
            }
            else
            {
                if constexpr (AlwaysHeapAllocated<T, S>)
                {
                    ptr.data_ = new T(std::forward<Args>(args)...);
                }
                else if constexpr (std::is_polymorphic_v<T> && !HasVirtualMove<T>)
                {
                    ptr.data_ = std::construct_at(reinterpret_cast<std::remove_cv_t<T> *>(ptr.buffer()),
                                                  std::forward<Args>(args)...);
                    ptr.move_ = move_buffer<std::remove_cv_t<T>>;
                }
                else
                {
                    ptr.data_ = std::construct_at(reinterpret_cast<std::remove_cv_t<T> *>(ptr.buffer()),
                                                  std::forward<Args>(args)...);
                }
            }

            return ptr;
        }

        template <typename T, std::size_t S>
        static constexpr SmallUniquePtr<T, S> invoke_array(std::size_t count)
        {
            SmallUniquePtr<T, S> ptr;

            if (AlwaysHeapAllocated<T, S> || (ptr.stack_array_size() < count) || std::is_constant_evaluated())
            {
                ptr.data_ = new std::remove_extent_t<T>[count] {
                };
            }
            else if constexpr (!AlwaysHeapAllocated<T, S>)
            {
                std::uninitialized_value_construct_n(ptr.buffer(), ptr.stack_array_size());
                ptr.data_ = std::launder(ptr.buffer());
            }

            return ptr;
        }

        template <typename T, typename B, std::size_t S>
        static constexpr SmallUniquePtr<B, S> invoke_for_overwrite_scalar() noexcept(
            std::is_nothrow_default_constructible_v<T> && !AlwaysHeapAllocated<T, S>)
        {
            SmallUniquePtr<B, S> ptr;

            if consteval
            {
                ptr.data_ = new T;
            }
            else
            {
                if constexpr (AlwaysHeapAllocated<T, S>)
                {
                    ptr.data_ = new T;
                }
                else if constexpr (std::is_polymorphic_v<T> && !HasVirtualMove<T>)
                {
                    ptr.data_ = ::new (static_cast<void *>(ptr.buffer())) std::remove_cv_t<T>;
                    ptr.move_ = move_buffer<std::remove_cv_t<T>>;
                }
                else
                {
                    ptr.data_ = ::new (static_cast<void *>(ptr.buffer())) std::remove_cv_t<T>;
                }
            }

            return ptr;
        }

        template <typename T, std::size_t S>
        static constexpr SmallUniquePtr<T, S> invoke_for_overwrite_array(std::size_t count)
        {
            SmallUniquePtr<T, S> ptr;

            if (AlwaysHeapAllocated<T, S> || (ptr.stack_array_size() < count) || std::is_constant_evaluated())
            {
                ptr.data_ = new std::remove_extent_t<T>[count];
            }
            else if constexpr (!AlwaysHeapAllocated<T, S>)
            {
                std::uninitialized_default_construct_n(ptr.buffer(), ptr.stack_array_size());
                ptr.data_ = std::launder(ptr.buffer());
            }

            return ptr;
        }
    };

    export template <typename T, std::size_t Size = default_small_ptr_size, typename... Args>
        requires(!std::is_array_v<T> && std::constructible_from<T, Args...>)
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small(Args &&...args) noexcept(
        std::is_nothrow_constructible_v<T, Args...> && !AlwaysHeapAllocated<T, Size>)
    {
        return MakeUniqueSmallImpl::invoke_scalar<T, T, Size>(std::forward<Args>(args)...);
    }

    export template <typename T, typename Base, std::size_t Size = default_small_ptr_size, typename... Args>
        requires(!std::is_array_v<T> && PointerConvertible<T, Base> && std::constructible_from<T, Args...>)
    [[nodiscard]] constexpr SmallUniquePtr<Base, Size> make_unique_small(Args &&...args) noexcept(
        std::is_nothrow_constructible_v<T, Args...> && !AlwaysHeapAllocated<T, Size>)
    {
        return MakeUniqueSmallImpl::invoke_scalar<T, Base, Size>(std::forward<Args>(args)...);
    }

    export template <typename T, std::size_t Size = default_small_ptr_size>
        requires std::is_unbounded_array_v<T>
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small(const std::size_t count)
    {
        return MakeUniqueSmallImpl::invoke_array<T, Size>(count);
    }

    export template <typename T, std::size_t Size = default_small_ptr_size, typename... Args>
        requires std::is_bounded_array_v<T>
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small(Args &&...args) = delete;

    export template <typename T, std::size_t Size = default_small_ptr_size>
        requires(!std::is_array_v<T>)
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small_for_overwrite() noexcept(
        std::is_nothrow_default_constructible_v<T> && !AlwaysHeapAllocated<T, Size>)
    {
        return MakeUniqueSmallImpl::invoke_for_overwrite_scalar<T, T, Size>();
    }

    export template <typename T, typename Base, std::size_t Size = default_small_ptr_size>
        requires(!std::is_array_v<T> && PointerConvertible<T, Base>)
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small_for_overwrite() noexcept(
        std::is_nothrow_default_constructible_v<T> && !AlwaysHeapAllocated<T, Size>)
    {
        return MakeUniqueSmallImpl::invoke_for_overwrite_scalar<T, Base, Size>();
    }

    export template <typename T, std::size_t Size = default_small_ptr_size>
        requires std::is_unbounded_array_v<T>
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small_for_overwrite(const std::size_t count)
    {
        return MakeUniqueSmallImpl::invoke_for_overwrite_array<T, Size>(count);
    }

    export template <typename T, std::size_t Size = default_small_ptr_size, typename... Args>
        requires std::is_bounded_array_v<T>
    [[nodiscard]] constexpr SmallUniquePtr<T, Size> make_unique_small_for_overwrite(Args &&...) = delete;
} // namespace retro
