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

    template <typename T, typename Member>
        requires std::is_member_function_pointer_v<Member> && std::is_object_v<T>
    struct MemberFunctionStorage
    {
        T *object;
        Member member;
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

        Delegate(const Delegate &) = delete;
        Delegate &operator=(const Delegate &) = delete;

        constexpr Delegate(Delegate &&other) noexcept
            : instance_(other.instance_), invoker_(other.invoker_), deleter_(other.deleter_)
        {
            other.instance_ = nullptr;
            other.invoker_ = nullptr;
            other.deleter_ = nullptr;
        }

        constexpr Delegate &operator=(Delegate &&other) noexcept
        {
            delete_data();

            instance_ = other.instance_;
            invoker_ = other.invoker_;
            deleter_ = other.deleter_;

            other.instance_ = nullptr;
            other.invoker_ = nullptr;
            other.deleter_ = nullptr;
            return *this;
        }

        ~Delegate()
        {
            delete_data();
        }

        constexpr bool is_bound() const noexcept
        {
            return invoker_ != nullptr;
        }

        constexpr void unbind()
        {
            delete_data();
            deleter_ = nullptr;
            invoker_ = nullptr;
            instance_ = nullptr;
        }

        constexpr Ret execute(Args &&...args) const
        {
            if (invoker_ == nullptr)
            {
                throw std::runtime_error("Delegate is not bound to a function");
            }

            return invoker_(instance_, std::forward<Args>(args)...);
        }

        constexpr bool execute_if_bound(Args... args) const
            requires std::same_as<Ret, void>
        {
            if (invoker_ == nullptr)
            {
                return false;
            }

            invoker_(instance_, std::forward<Args>(args)...);
            return true;
        }
        constexpr boost::optional<Ret> execute_if_bound(Args... args) const
            requires !std::same_as<Ret, void>
        {
            if (invoker_ == nullptr)
            {
                return boost::none;
            }

            return invoker_(instance_, std::forward<Args>(args)...);
        }

        template <Ret (*Func)(Args...)>
        void bind_static()
        {
            delete_data();
            instance_ = nullptr;
            invoker_ = &invoke_static<Func>;
            deleter_ = nullptr;
        }

        void bind_static(Ret (*func)(Args...))
        {
            delete_data();
            instance_ = func;
            invoker_ = &invoke_static_runtime;
            deleter_ = nullptr;
        }

        template <auto MemberPtr, typename T>
            requires std::is_object_v<T> && std::is_member_function_pointer_v<decltype(MemberPtr)> &&
                     std::invocable<decltype(MemberPtr), T, Args...>
        void bind_raw(T &object)
        {
            delete_data();
            instance_ = &object;
            invoker_ = &invoke_member<T, MemberPtr>;
            deleter_ = nullptr;
        }

        template <typename T, typename Member>
            requires std::is_object_v<T> && std::is_member_function_pointer_v<Member> &&
                     std::invocable<Member, T, Args...>
        void bind_raw(T &object, Member member)
        {
            delete_data();
            instance_ = new MemberFunctionStorage<T, Member>{&object, member};
            invoker_ = &invoke_member_raw<T, Member>;
            deleter_ = &delete_member_raw<T, Member>;
        }

        template <typename Functor>
            requires std::invocable<Functor, Args...>
        void bind_lambda(Functor &&functor)
        {
            using DecayedLambda = std::decay_t<Functor>;
            if constexpr (std::is_function_v<DecayedLambda>)
            {
                bind_static(std::forward<Functor>(functor));
            }
            else
            {
                delete_data();
                auto *heap_object = new DecayedLambda(std::forward<Functor>(functor));
                instance_ = heap_object;
                invoker_ = &invoke_functor<DecayedLambda>;
                deleter_ = &delete_functor<DecayedLambda>;
            }
        }

      private:
        void delete_data() const
        {
            if (deleter_ != nullptr && instance_ != nullptr)
            {
                deleter_(instance_);
            }
        }

        template <Ret (*Func)(Args...)>
        static Ret invoke_static(void *, Args... args)
        {
            return Func(std::forward<Args>(args)...);
        }

        static Ret invoke_static_runtime(void *data, Args... args)
        {
            auto *func_ptr = static_cast<Ret (*)(Args...)>(data);
            return func_ptr(std::forward<Args>(args)...);
        }

        template <typename T, auto MemberPtr>
        static Ret invoke_member(void *data, Args... args)
        {
            auto *object = static_cast<T *>(data);
            return std::invoke(MemberPtr, object, std::forward<Args>(args)...);
        }

        template <typename T, typename Member>
        static Ret invoke_member_raw(void *data, Args... args)
        {
            auto *invoker = static_cast<MemberFunctionStorage<T, Member> *>(data);
            return std::invoke(invoker->member, invoker->object, std::forward<Args>(args)...);
        }

        template <typename T, typename Member>
        static void delete_member_raw(void *data)
        {
            auto *invoker = static_cast<MemberFunctionStorage<T, Member> *>(data);
            delete invoker;
        }

        template <typename Functor>
        static Ret invoke_functor(void *obj, Args... args)
        {
            auto *functor = static_cast<Functor *>(obj);
            return (*functor)(std::forward<Args>(args)...);
        }

        template <typename Functor>
        static void delete_functor(void *obj)
        {
            auto *functor = static_cast<Functor *>(obj);
            delete functor;
        }

        void *instance_ = nullptr;
        Ret (*invoker_)(void *, Args...) = nullptr;
        void (*deleter_)(void *) = nullptr;
    };
} // namespace retro
