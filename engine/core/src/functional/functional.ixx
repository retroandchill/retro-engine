/**
 * @file functional.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:functional;

import :concepts;
import boost;

namespace retro
{
    export template <CallableObject... Ts>
    struct Overload : Ts...
    {
        using Ts::operator()...;
    };

    export template <CallableObject... Ts>
    Overload(Ts...) -> Overload<Ts...>;

    static constexpr usize DELEGATE_INLINE_ALIGN = alignof(std::max_align_t);
    static constexpr usize DELEGATE_INLINE_SIZE = 32;

    union DelegateStorage
    {
        alignas(DELEGATE_INLINE_ALIGN) std::byte inline_buffer[DELEGATE_INLINE_SIZE];
        void *heap_object{};
    };

    export template <typename>
    class Delegate;

    export template <typename Ret, typename... Args>
    class Delegate<Ret(Args...)>
    {
      public:
        using ReturnType = Ret;
        using Signature = Ret(Args...);

        constexpr Delegate() = default;
        constexpr explicit(false) Delegate(std::nullptr_t)
        {
        }

        Delegate(const Delegate &other) : ops_(other.ops_), object_size_(other.object_size_)
        {
            copy_data();
        }

        Delegate &operator=(const Delegate &other)
        {
            if (this == std::addressof(other))
                return *this;

            delete_data();

            ops_ = other.ops_;
            object_size_ = other.object_size_;
            copy_data(other);
            return *this;
        }

        constexpr Delegate(Delegate &&other) noexcept : ops_(other.ops_), object_size_(other.object_size_)
        {
            move_data(std::move(other));
        }

        constexpr Delegate &operator=(Delegate &&other) noexcept
        {
            delete_data();

            ops_ = other.ops_;
            object_size_ = other.object_size_;
            move_data(std::move(other));
            return *this;
        }

        ~Delegate()
        {
            delete_data();
        }

        [[nodiscard]] constexpr bool is_bound() const noexcept
        {
            return ops_ != nullptr;
        }

        constexpr void unbind()
        {
            delete_data();
            ops_ = nullptr;
            object_size_ = 0;
        }

        constexpr Ret execute(Args &&...args)
        {
            if (ops_ == nullptr)
            {
                throw std::runtime_error("Delegate is not bound to a function");
            }

            return ops_->invoke(storage_, std::forward<Args>(args)...);
        }

        constexpr bool execute_if_bound(Args... args)
            requires std::same_as<Ret, void>
        {
            if (ops_ == nullptr)
            {
                return false;
            }

            ops_->invoke(storage_, std::forward<Args>(args)...);
            return true;
        }
        constexpr boost::optional<Ret> execute_if_bound(Args... args)
            requires !std::same_as<Ret, void>
        {
            if (ops_ == nullptr)
            {
                return boost::none;
            }

            return ops_->invoke(storage_, std::forward<Args>(args)...);
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        void bind(Functor &&functor) noexcept
        {
            delete_data();

            ops_ = get_ops_table<std::remove_cvref_t<Functor>>();
            object_size_ = sizeof(Functor);
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                auto *functor_ptr =
                    std::launder(reinterpret_cast<std::remove_cvref_t<Functor> *>(&storage_.inline_buffer));
                std::construct_at(functor_ptr, std::forward<Functor>(functor));
            }
            else
            {
                storage_.heap_object = new Functor(std::forward<Functor>(functor));
            }
        }

        template <auto Functor>
            requires std::invocable<decltype(Functor), Args...> &&
                     std::convertible_to<std::invoke_result_t<decltype(Functor), Args...>, Ret>
        void bind() noexcept
        {
            bind([]<typename... A>(A &&...args) { return std::invoke(Functor, std::forward<A>(args)...); });
        }

        template <typename T, std::invocable<T, Args...> Member>
            requires std::convertible_to<std::invoke_result_t<Member, T, Args...>, Ret> &&
                     std::is_member_pointer_v<Member>
        void bind(T &obj, Member member) noexcept
        {
            bind(std::bind_front(member, std::ref(obj)));
        }

        template <auto Member, typename T>
            requires std::convertible_to<std::invoke_result_t<decltype(Member), T, Args...>, Ret> &&
                     std::is_member_pointer_v<decltype(Member)>
        void bind(T &obj) noexcept
        {
            bind([&obj]<typename... A>(A &&...args) { return std::invoke(Member, obj, std::forward<A>(args)...); });
        }

      private:
        struct OpsTable
        {
            Ret (*invoke)(DelegateStorage &storage, Args... args) = nullptr;
            void (*copy)(DelegateStorage &dest, const DelegateStorage &src) = nullptr;
            void (*destroy)(DelegateStorage &) = nullptr;
        };

        [[nodiscard]] constexpr bool is_inline() const noexcept
        {
            return object_size_ <= DELEGATE_INLINE_SIZE;
        }

        void copy_data(const Delegate &other)
        {
            if (ops_ == nullptr)
                return;

            if (ops_->copy != nullptr)
            {
                ops_->copy(storage_, other.storage_);
            }
            else if (is_inline())
            {
                std::memcpy(storage_.inline_buffer, other.storage_.inline_buffer, sizeof(storage_.inline_buffer));
            }
        }

        void move_data(Delegate &&other)
        {
            if (ops_ == nullptr)
                return;

            if (is_inline())
            {
                std::memcpy(storage_.inline_buffer, other.storage_.inline_buffer, sizeof(storage_.inline_buffer));
            }
            else
            {
                storage_.heap_object = other.storage_.heap_object;
                other.storage_.heap_object = nullptr;
            }

            other.ops_ = nullptr;
            other.object_size_ = 0;
        }

        void delete_data()
        {
            if (ops_ != nullptr && ops_->destroy != nullptr)
            {
                ops_->destroy(storage_);
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static OpsTable *get_ops_table()
        {
            static constexpr bool requires_copy =
                !std::is_trivially_copyable_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE;
            static constexpr bool requires_destroy =
                !std::is_trivially_destructible_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE;

            if constexpr (requires_copy && requires_destroy)
            {
                static OpsTable ops_table{.invoke = invoke_functor<Functor>,
                                          .copy = copy_functor<Functor>,
                                          .destroy = delete_functor<Functor>};
                return &ops_table;
            }
            else if constexpr (requires_copy)
            {
                static OpsTable ops_table{.invoke = invoke_functor<Functor>, .copy = copy_functor<Functor>};
                return &ops_table;
            }
            else if constexpr (requires_destroy)
            {
                static OpsTable ops_table{.invoke = invoke_functor<Functor>, .destroy = delete_functor<Functor>};
                return &ops_table;
            }
            else
            {
                static OpsTable ops_table{
                    .invoke = invoke_functor<Functor>,
                };
                return &ops_table;
            }
        }

        template <typename Functor>
        static Ret invoke_functor(DelegateStorage &obj, Args... args)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                return std::invoke(*std::launder(reinterpret_cast<Functor *>(&obj.inline_buffer)),
                                   std::forward<Args>(args)...);
            }
            else
            {
                return std::invoke(*static_cast<Functor *>(obj.heap_object), std::forward<Args>(args)...);
            }
        }

        template <typename Functor>
        static void copy_functor(DelegateStorage &dest, const DelegateStorage &source)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                std::construct_at(reinterpret_cast<Functor *>(dest.inline_buffer),
                                  *reinterpret_cast<const Functor *>(source.inline_buffer));
            }
            else
            {
                dest.heap_object = new Functor(*static_cast<const Functor *>(source.heap_object));
            }
        }

        template <typename Functor>
        static void delete_functor(DelegateStorage &obj)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                std::destroy_at(reinterpret_cast<Functor *>(obj.inline_buffer));
            }
            else
            {
                delete static_cast<Functor *>(obj.heap_object);
            }
        }

        DelegateStorage storage_;
        const OpsTable *ops_ = nullptr;
        usize object_size_ = 0;
    };
} // namespace retro
