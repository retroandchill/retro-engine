/**
 * @file polymorphic.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

export module retro.core:containers.polymorphic;

import :defines;
import std;

namespace retro
{
    export enum class PolymorphicType : uint8
    {
        Copyable,
        MoveOnly,
        NonCopyable
    };

    template <usize Size>
    // NOLINTNEXTLINE
    union PolymorphicStorage
    {
        std::array<std::byte, Size> small_buffer;
        void *large_buffer;

        template <typename T, typename... A>
            requires std::constructible_from<T, A...>
        constexpr void emplace(A &&...Args) noexcept
        {
            if constexpr (sizeof(T) <= Size)
            {
                std::construct_at(reinterpret_cast<T *>(small_buffer.data()), std::forward<A>(Args)...);
            }
            else
            {
                large_buffer = new T(std::forward<A>(Args)...);
            }
        }
    };

    template <PolymorphicType Type, usize Size>
    struct VTable;

    template <usize Size>
    struct VTable<PolymorphicType::Copyable, Size>
    {
        const std::type_info &(*get_type)() = nullptr;
        usize size = 0;
        void *(*get_value)(PolymorphicStorage<Size> &storage) = nullptr;
        const void *(*get_const_value)(const PolymorphicStorage<Size> &storage) = nullptr;
        void (*destroy)(PolymorphicStorage<Size> &storage) = nullptr;
        void (*copy)(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
        void (*copy_assign)(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
        void (*move)(PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
        void (*move_assign)(PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
    };

    template <usize Size>
    struct VTable<PolymorphicType::MoveOnly, Size>
    {
        const std::type_info &(*get_type)() = nullptr;
        usize size = 0;
        void *(*get_value)(PolymorphicStorage<Size> &storage) = nullptr;
        const void *(*get_const_value)(const PolymorphicStorage<Size> &storage) = nullptr;
        void (*destroy)(PolymorphicStorage<Size> &storage) = nullptr;
        void (*copy)(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
        void (*copy_assign)(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest) = nullptr;
    };

    template <usize Size>
    struct VTable<PolymorphicType::NonCopyable, Size>
    {
        const std::type_info &(*get_type)() = nullptr;
        usize size = 0;
        void *(*get_value)(PolymorphicStorage<Size> &storage) = nullptr;
        const void *(*get_const_value)(const PolymorphicStorage<Size> &storage) = nullptr;
        void (*destroy)(PolymorphicStorage<Size> &storage) = nullptr;
    };

    template <typename T, usize Size>
    struct VTableImpl
    {
        static constexpr bool FITS_STORAGE_BUFFER = sizeof(T) <= Size;

        static constexpr const std::type_info &get_type()
        {
            return typeid(T);
        }

        static constexpr void *get_value(PolymorphicStorage<Size> &Data)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                return reinterpret_cast<T *>(Data.small_buffer.data());
            }
            else
            {
                return static_cast<T *>(Data.large_buffer);
            }
        }

        static constexpr const void *get_const_value(const PolymorphicStorage<Size> &Data)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                return reinterpret_cast<const T *>(Data.small_buffer.data());
            }
            else
            {
                return static_cast<const T *>(Data.large_buffer);
            }
        }

        static constexpr void destroy(PolymorphicStorage<Size> &Data)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                std::destroy_at(reinterpret_cast<T *>(Data.small_buffer.data()));
            }
            else
            {
                delete static_cast<const T *>(Data.large_buffer);
            }
        }

        static constexpr void copy(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest)
            requires(std::is_copy_constructible_v<T>)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                dest.template emplace<T>(*reinterpret_cast<const T *>(src.small_buffer.data()));
            }
            else
            {
                dest.template emplace<T>(*static_cast<const T *>(src.large_buffer));
            }
        }

        static constexpr void copy_assign(const PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest)
            requires(std::is_copy_assignable_v<T>)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                *reinterpret_cast<T *>(dest.small_buffer.data()) =
                    *reinterpret_cast<const T *>(src.small_buffer.data());
            }
            else
            {
                *static_cast<T *>(dest.large_buffer) = *static_cast<const T *>(src.large_buffer);
            }
        }

        static constexpr void move(PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest)
            requires(std::is_move_constructible_v<T>)
        {
            if constexpr (FITS_STORAGE_BUFFER)
            {
                dest.template emplace<T>(std::move(*reinterpret_cast<T *>(src.small_buffer.data())));
            }
            else
            {
                dest.template emplace<T>(std::move(*static_cast<T *>(src.large_buffer)));
            }
        }

        static constexpr void move_assign(PolymorphicStorage<Size> &src, PolymorphicStorage<Size> &dest)
            requires(std::is_move_assignable_v<T>)
        {
            // NOLINTNEXTLINE
            if constexpr (FITS_STORAGE_BUFFER)
            {
                *reinterpret_cast<T *>(dest.small_buffer.data()) =
                    std::move(*reinterpret_cast<T *>(src.small_buffer.data()));
            }
            else
            {
                *static_cast<T *>(dest.large_buffer) = std::move(*static_cast<T *>(src.large_buffer));
            }
        }
    };

    template <typename T, PolymorphicType Type, usize Size>
    static const VTable<Type, Size> *get_vtable()
    {
        using Impl = VTableImpl<T, Size>;
        if constexpr (Type == PolymorphicType::Copyable)
        {
            static constexpr VTable<Type, Size> VTable = {.get_type = Impl::get_type,
                                                          .size = sizeof(T),
                                                          .get_value = Impl::get_value,
                                                          .get_const_value = Impl::get_const_value,
                                                          .destroy = Impl::destroy,
                                                          .copy = Impl::copy,
                                                          .copy_assign = Impl::copy_assign,
                                                          .move = Impl::move,
                                                          .move_assign = Impl::move_assign};
            return &VTable;
        }
        else if constexpr (Type == PolymorphicType::MoveOnly)
        {
            static constexpr VTable<Type, Size> VTable = {.get_type = Impl::get_type,
                                                          .size = sizeof(T),
                                                          .get_value = Impl::get_value,
                                                          .get_const_value = Impl::get_const_value,
                                                          .destroy = Impl::destroy,
                                                          .move = Impl::move,
                                                          .move_assign = Impl::move_assign};
            return &VTable;
        }
        else
        {
            static constexpr VTable<Type, Size> VTable = {.get_type = Impl::get_type,
                                                          .size = sizeof(T),
                                                          .get_value = Impl::get_value,
                                                          .get_const_value = Impl::get_const_value,
                                                          .destroy = Impl::destroy};
            return &VTable;
        }
    }

    constexpr usize BASE_BUFFER_SIZE = 56;

    template <typename T>
    struct DefaultBufferSize;

    template <typename T>
        requires(sizeof(T) <= BASE_BUFFER_SIZE)
    struct DefaultBufferSize<T> : std::integral_constant<usize, BASE_BUFFER_SIZE>
    {
    };

    template <typename T>
        requires(sizeof(T) > BASE_BUFFER_SIZE)
    struct DefaultBufferSize<T> : std::integral_constant<usize, sizeof(T)>
    {
    };

    template <typename T>
    constexpr usize DEFAULT_BUFFER_SIZE = DefaultBufferSize<T>::value;

    export template <typename T, PolymorphicType Type = PolymorphicType::Copyable, usize Size = DEFAULT_BUFFER_SIZE<T>>
        requires(Size >= sizeof(T *))
    class Polymorphic
    {
        template <typename U>
            requires std::derived_from<U, T>
        static constexpr bool FITS_STORAGE_BUFFER = sizeof(U) <= Size;

      public:
        static constexpr bool IS_COPYABLE = Type == PolymorphicType::Copyable;
        static constexpr bool IS_MOVABLE = IS_COPYABLE || Type == PolymorphicType::MoveOnly;

        constexpr Polymorphic()
            requires(std::is_default_constructible_v<T>)
            : vtable_(get_vtable<T, Type, Size>())
        {
            storage_.emplace();
        }

        template <std::derived_from<T> U>
            requires std::constructible_from<std::remove_cvref_t<U>, U>
        constexpr explicit Polymorphic(U &&other) : vtable_(get_vtable<U, Type, Size>())
        {
            storage_.template emplace<std::remove_cvref_t<U>>(std::forward<U>(other));
        }

        template <std::derived_from<T> U, typename... Args>
            requires std::constructible_from<U, Args...>
        constexpr explicit Polymorphic(std::in_place_type_t<U>, Args &&...args) : vtable_(get_vtable<U, Type, Size>())
        {
            storage_.template emplace<U>(std::forward<Args>(args)...);
        }

        constexpr Polymorphic(const Polymorphic &other)
            requires(IS_COPYABLE)
            : vtable_(other.vtable_)
        {
            vtable_->copy(other.storage_, storage_);
        }

        constexpr Polymorphic(const Polymorphic &) noexcept
            requires(!IS_COPYABLE)
        = delete;

        constexpr Polymorphic(Polymorphic &&other) noexcept
            requires(IS_MOVABLE)
            : vtable_(other.vtable_)
        {
            vtable_->move(other.storage_, storage_);
        }

        constexpr Polymorphic(Polymorphic &&) noexcept
            requires(!IS_MOVABLE)
        = delete;

        ~Polymorphic()
        {
            vtable_->destroy(storage_);
        }

        constexpr Polymorphic &operator=(const Polymorphic &other)
            requires(IS_COPYABLE)
        {
            if (this == &other)
                return *this;

            if (vtable_ != other.vtable_)
            {
                vtable_->destroy(storage_);
                vtable_ = other.vtable_;
                vtable_->copy(other.storage_, storage_);
            }
            else
            {
                vtable_->copy_assign(other.storage_, storage_);
            }

            return *this;
        }

        constexpr Polymorphic &operator=(const Polymorphic &) noexcept
            requires(!IS_COPYABLE)
        = delete;

        constexpr Polymorphic &operator=(Polymorphic &&other) noexcept
            requires(IS_MOVABLE)
        {
            if (vtable_ != other.vtable_)
            {
                vtable_->destroy(storage_);
                vtable_ = other.vtable_;
                vtable_->move(other.storage_, storage_);
            }
            else
            {
                vtable_->move_assign(other.storage_, storage_);
            }

            return *this;
        }

        constexpr Polymorphic &operator=(Polymorphic &&) noexcept
            requires(!IS_MOVABLE)
        = delete;

        [[nodiscard]] constexpr const std::type_info &type() const noexcept
        {
            return vtable_->get_type();
        }

        [[nodiscard]] constexpr T &get() noexcept
        {
            return *static_cast<T *>(vtable_->get_value(storage_));
        }

        [[nodiscard]] constexpr const T &get() const noexcept
        {
            return *static_cast<const T *>(vtable_->get_const_value(storage_));
        }

        [[nodiscard]] constexpr usize size() const noexcept
        {
            return vtable_->size;
        }

        template <typename Self>
        constexpr auto operator->(this Self &self) noexcept
        {
            return &self.get();
        }

        template <typename Self>
        constexpr decltype(auto) operator*(this Self &self) noexcept
        {
            return self.get();
        }

        template <std::derived_from<T> U, typename... Args>
            requires IS_MOVABLE && std::constructible_from<U, Args...>
        constexpr U &emplace(Args &&...args) noexcept
        {
            if (typeid(U) != type())
            {
                vtable_->destroy(storage_);
                vtable_ = get_vtable<U, Type, Size>();
                storage_.template emplace<U>(std::forward<Args>(args)...);
            }
            else
            {
                static_cast<U &>(get()) = U{std::forward<Args>(args)...};
            }

            return *this;
        }

      private:
        PolymorphicStorage<Size> storage_;
        const VTable<Type, Size> *vtable_;
    };
} // namespace retro
