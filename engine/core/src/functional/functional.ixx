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

    template <auto Functor>
        requires CallableObject<decltype(Functor)>
    struct ConstantBinding
    {
        template <typename... Args>
            requires std::invocable<decltype(Functor), Args...>
        constexpr decltype(auto) operator()(Args &&...args) const
            noexcept(std::is_nothrow_invocable_v<decltype(Functor), Args...>)
        {
            return std::invoke(Functor, std::forward<Args>(args)...);
        }
    };

    export template <auto Functor, typename... Args>
    constexpr auto bind_front(Args &&...args)
    {
        return std::bind_front(ConstantBinding<Functor>{}, std::forward<Args>(args)...);
    }

    export template <auto Functor, typename... Args>
    constexpr auto bind_back(Args &&...args)
    {
        return std::bind_back(ConstantBinding<Functor>{}, std::forward<Args>(args)...);
    }

    template <typename T>
    concept DirectMemberBindable = std::is_object_v<std::remove_cvref_t<T>> && std::is_lvalue_reference_v<T>;

    template <typename T>
    concept MemberBindable =
        DirectMemberBindable<T> || ((SharedPtrLike<T> || WeakPtrLike<T>)&&std::is_object_v<PointerElementT<T>>);

    template <MemberBindable>
    struct MemberBinding;

    template <DirectMemberBindable T>
    struct MemberBinding<T>
    {
        using Type = T;
    };

    template <typename T>
        requires SharedPtrLike<T> || WeakPtrLike<T>
    struct MemberBinding<T>
    {
        using Type = PointerElementT<T> &;
    };

    template <MemberBindable T>
    using MemberBindingT = MemberBinding<T>::Type;

    template <WeakPtrLike T, typename Functor>
        requires std::is_member_pointer_v<Functor>
    struct WeakFunctionBinding
    {
        constexpr WeakFunctionBinding(T object, Functor functor)
            : object_(std::move(object)), functor_(std::move(functor))
        {
        }

        template <typename... Args>
            requires std::invocable<Functor, PointerElementT<T> &, Args...>
        constexpr decltype(auto) operator()(Args &&...args) noexcept(std::is_nothrow_invocable_v<Functor, T &, Args...>)
        {
            return std::invoke(functor_, *object_.lock(), std::forward<Args>(args)...);
        }

        constexpr bool is_bound() const noexcept
        {
            return !object_.expired();
        }

      private:
        T object_;
        Functor functor_;
    };

    template <WeakPtrLike T, auto Functor>
        requires std::is_member_pointer_v<decltype(Functor)>
    struct ConstWeakFunctionBinding
    {
        constexpr explicit ConstWeakFunctionBinding(T object) : object_(std::move(object))
        {
        }

        template <typename... Args>
            requires std::invocable<decltype(Functor), PointerElementT<T> &, Args...>
        constexpr decltype(auto) operator()(Args &&...args) noexcept(
            std::is_nothrow_invocable_v<decltype(Functor), T &, Args...>)
        {
            return std::invoke(Functor, *object_.lock(), std::forward<Args>(args)...);
        }

        constexpr bool is_bound() const noexcept
        {
            return !object_.expired();
        }

      private:
        T object_;
    };

    template <typename>
    struct IsWeakFunctionBinding : std::false_type
    {
    };

    template <WeakPtrLike T, typename Functor>
        requires std::is_member_pointer_v<Functor>
    struct IsWeakFunctionBinding<WeakFunctionBinding<T, Functor>> : std::true_type
    {
    };

    template <typename T>
    concept WeakFunctionBindingLike = IsWeakFunctionBinding<std::remove_cvref_t<T>>::value;

    static constexpr usize DELEGATE_INLINE_ALIGN = alignof(std::max_align_t);
    static constexpr usize DELEGATE_INLINE_SIZE = 16;

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

        Delegate(const Delegate &other) : ops_(other.ops_)
        {
            copy_data();
        }

        Delegate &operator=(const Delegate &other)
        {
            if (this == std::addressof(other))
                return *this;

            delete_data();

            ops_ = other.ops_;
            copy_data(other);
            return *this;
        }

        constexpr Delegate(Delegate &&other) noexcept : ops_(other.ops_)
        {
            move_data(std::move(other));
        }

        constexpr Delegate &operator=(Delegate &&other) noexcept
        {
            delete_data();

            ops_ = other.ops_;
            move_data(std::move(other));
            return *this;
        }

        ~Delegate()
        {
            delete_data();
        }

        [[nodiscard]] constexpr bool is_bound() const noexcept
        {
            return ops_ != nullptr && (ops_->is_bound == nullptr || ops_->is_bound(storage_));
        }

        constexpr void unbind()
        {
            delete_data();
            ops_ = nullptr;
        }

        constexpr Ret execute(Args &&...args)
        {
            if (!is_bound())
            {
                throw std::bad_function_call{};
            }

            return ops_->invoke(storage_, std::forward<Args>(args)...);
        }

        constexpr bool execute_if_bound(Args... args)
            requires std::same_as<Ret, void>
        {
            if (!is_bound())
            {
                return false;
            }

            ops_->invoke(storage_, std::forward<Args>(args)...);
            return true;
        }
        constexpr boost::optional<Ret> execute_if_bound(Args... args)
            requires !std::same_as<Ret, void>
        {
            if (!is_bound())
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
            bind(ConstantBinding<Functor>{});
        }

        template <MemberBindable T, std::invocable<T, Args...> Member>
            requires std::convertible_to<std::invoke_result_t<Member, MemberBindingT<T>, Args...>, Ret> &&
                     std::is_member_pointer_v<Member>
        void bind(T &&obj, Member member) noexcept
        {
            if constexpr (WeakBindable<T>)
            {
                using WeakType = std::remove_cvref_t<decltype(to_weak(std::forward<T>(obj)))>;
                bind(WeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj)), member});
            }
            else
            {
                bind(std::bind_front(member, std::ref(obj)));
            }
        }

        template <auto Member, MemberBindable T>
            requires std::convertible_to<std::invoke_result_t<decltype(Member), MemberBindingT<T>, Args...>, Ret> &&
                     std::is_member_pointer_v<decltype(Member)>
        void bind(T &&obj) noexcept
        {
            if constexpr (WeakBindable<T>)
            {
                using WeakType = std::remove_cvref_t<decltype(to_weak(std::forward<T>(obj)))>;
                bind(ConstWeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj))});
            }
            else
            {
                bind(bind_front<Member>(std::ref(obj)));
            }
        }

      private:
        struct OpsTable
        {
            usize object_size{};
            Ret (*invoke)(DelegateStorage &storage, Args... args) = nullptr;
            void (*copy)(DelegateStorage &dest, const DelegateStorage &src) = nullptr;
            void (*destroy)(DelegateStorage &) = nullptr;
            bool (*is_bound)(const DelegateStorage &) = nullptr;
        };

        [[nodiscard]] constexpr bool is_inline() const noexcept
        {
            return ops_ != nullptr && ops_->object_size <= DELEGATE_INLINE_SIZE;
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
            static OpsTable ops_table{.object_size = sizeof(Functor),
                                      .invoke = invoke_functor<Functor>,
                                      .copy = get_copy_operation<Functor>(),
                                      .destroy = get_delete_operation<Functor>(),
                                      .is_bound = get_bound_check<Functor>()};
            return &ops_table;
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_copy_operation()
        {
            if constexpr (!std::is_trivially_copyable_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE)
            {
                return &copy_functor<Functor>;
            }
            else
            {
                return nullptr;
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_delete_operation()
        {
            if constexpr (!std::is_trivially_destructible_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE)
            {
                return &delete_functor<Functor>;
            }
            else
            {
                return nullptr;
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_bound_check()
        {
            if constexpr (WeakFunctionBindingLike<Functor>)
            {
                return &is_functor_bound<Functor>;
            }
            else
            {
                return nullptr;
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

        template <WeakFunctionBindingLike Functor>
        static bool is_functor_bound(const DelegateStorage &obj)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                return std::launder(reinterpret_cast<Functor *>(&obj.inline_buffer))->is_bound();
            }
            else
            {
                return static_cast<const Functor *>(obj.heap_object)->is_bound();
            }
        }

        DelegateStorage storage_;
        const OpsTable *ops_ = nullptr;
    };

    export using SimpleDelegate = Delegate<void()>;
} // namespace retro
