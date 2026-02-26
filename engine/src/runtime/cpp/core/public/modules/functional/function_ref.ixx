/**
 * @file function_ref.ixx
 *
 * BSD 2-Clause License
 *
 * Copyright (c) 2022, Zhihao Yuan
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
module;

#include <cassert>

export module retro.core.functional.function_ref;

import std;

namespace retro
{
    export template <auto V>
    struct Nontype
    {
    };

    export template <auto V>
    constexpr Nontype<V> nontype{};

    template <typename T>
    constexpr auto select_param_type = []
    {
        if constexpr (std::is_trivially_copyable_v<T>)
            return std::type_identity<T>();
        else
            return std::add_rvalue_reference<T>();
    };

    template <typename T>
    using ParamType = std::invoke_result_t<decltype(select_param_type<T>)>::type;

    template <typename T, typename Self>
    concept NotSelf = !std::same_as<std::remove_cvref_t<T>, Self>;

    template <typename T, template <typename...> typename>
    constexpr bool looks_nullable_to_impl = std::is_member_pointer_v<T>;

    template <typename F, template <typename...> typename Self>
    constexpr bool looks_nullable_to_impl<F *, Self> = std::is_function_v<F>;

    template <typename... S, template <typename...> typename Self>
    constexpr bool looks_nullable_to_impl<Self<S...>, Self> = true;

    template <typename S, template <typename...> typename Self>
    concept LooksNullableTo = looks_nullable_to_impl<std::remove_cvref_t<S>, Self>;

    template <typename T>
    constexpr bool is_not_nontype = true;

    template <auto F>
    constexpr bool is_not_nontype<Nontype<F>> = false;

    template <typename T>
    struct AdaptSignature;

    template <typename F>
        requires std::is_function_v<F>
    struct AdaptSignature<F>
    {
        using Type = F;
    };

    template <typename Fp>
    using AdaptSignatureType = AdaptSignature<Fp>::Type;

    template <typename S>
    struct NotQualifyingThis
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...)>
    {
        using Type = R(Args...);
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) noexcept>
    {
        using Type = R(Args...) noexcept;
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) &> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const &> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile &> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile &> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) &&> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const &&> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile &&> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile &&> : NotQualifyingThis<R(Args...)>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) & noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const & noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile & noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile & noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) && noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const && noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) volatile && noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename R, typename... Args>
    struct NotQualifyingThis<R(Args...) const volatile && noexcept> : NotQualifyingThis<R(Args...) noexcept>
    {
    };

    template <typename F, typename T>
    struct DropFirstArgToInvoke;

    template <typename T, typename R, typename G, typename... Args>
    struct DropFirstArgToInvoke<R (*)(G, Args...), T>
    {
        using Type = R(Args...);
    };

    template <typename T, typename R, typename G, typename... Args>
    struct DropFirstArgToInvoke<R (*)(G, Args...) noexcept, T>
    {
        using Type = R(Args...) noexcept;
    };

    template <typename T, typename M, typename G>
        requires std::is_object_v<M>
    struct DropFirstArgToInvoke<M G::*, T>
    {
        using Type = std::invoke_result_t<M G::*, T>() noexcept;
    };

    template <typename T, typename M, typename G>
        requires std::is_function_v<M>
    struct DropFirstArgToInvoke<M G::*, T> : NotQualifyingThis<M>
    {
    };

    template <typename F, typename T>
    using DropFirstArgToInvokeType = DropFirstArgToInvoke<std::remove_cvref_t<F>, T>::Type;

    template <typename Sig>
    struct QualFnSig;

    template <typename R, typename... Args>
    struct QualFnSig<R(Args...)>
    {
        using Function = R(Args...);
        using WithoutNoexcept = Function;
        static constexpr bool is_noexcept = false;

        template <typename... T>
        static constexpr bool is_invocable_using = std::is_invocable_r_v<R, T..., Args...>;

        template <typename T>
        using CvType = T;
    };

    template <typename R, typename... Args>
    struct QualFnSig<R(Args...) noexcept>
    {
        using Function = R(Args...);
        using WithoutNoexcept = Function;
        static constexpr bool is_noexcept = true;

        template <typename... T>
        static constexpr bool is_invocable_using = std::is_nothrow_invocable_r_v<R, T..., Args...>;

        template <typename T>
        using CvType = T;
    };

    template <typename R, typename... Args>
    struct QualFnSig<R(Args...) const> : QualFnSig<R(Args...)>
    {
        template <typename T>
        using CvType = const T;

        using WithoutNoexcept = R(Args...) const;
    };

    template <typename R, typename... Args>
    struct QualFnSig<R(Args...) const noexcept> : QualFnSig<R(Args...) noexcept>
    {
        template <typename T>
        using CvType = const T;

        using WithoutNoexcept = R(Args...) const;
    };

    struct FunctionRefBase
    {
        union Storage
        {
            void *ptr = nullptr;
            const void *const_ptr;
            void (*function_ptr)();

            constexpr Storage() noexcept = default;

            template <typename T>
                requires std::is_object_v<T>
            constexpr explicit Storage(T *p) noexcept : ptr(p)
            {
            }

            template <typename T>
                requires std::is_object_v<T>
            constexpr explicit Storage(const T *p) noexcept : const_ptr(p)
            {
            }

            template <typename T>
                requires std::is_function_v<T>
            constexpr explicit Storage(T *p) noexcept : function_ptr(reinterpret_cast<decltype(function_ptr)>(p))
            {
            }
        };

        template <typename T>
        constexpr static auto get(Storage obj)
        {
            if constexpr (std::is_const_v<T>)
                return static_cast<T *>(obj.const_ptr);
            else if constexpr (std::is_object_v<T>)
                return static_cast<T *>(obj.ptr);
            else
                return reinterpret_cast<T *>(obj.function_ptr);
        }
    };

    export template <typename Sig, typename = typename QualFnSig<Sig>::Function>
    class FunctionRef;

    template <typename From, typename To>
    constexpr bool is_ref_convertible = false;

    template <typename T, typename U>
    constexpr bool is_ref_convertible<FunctionRef<T>, FunctionRef<U>> =
        std::is_convertible_v<typename NotQualifyingThis<T>::Type &, typename NotQualifyingThis<U>::Type &>;

    export template <typename Sig, typename R, typename... Args>
    class FunctionRef<Sig, R(Args...)> : public FunctionRefBase
    {
        using Signature = QualFnSig<Sig>;

        template <typename T>
        using CvType = Signature::template CvType<T>;

        template <typename T>
        using CvRefType = CvType<T> &;

        static constexpr bool Noexcept = Signature::is_noexcept;

        template <typename... T>
        static constexpr bool is_invocable_using = Signature::template is_invocable_using<T...>;

        template <typename F>
        static constexpr bool is_convertible_from_specialization = is_ref_convertible<F, FunctionRef>;

        using ForwardType = R(Storage, ParamType<Args>...) noexcept(Noexcept);

        ForwardType *function_ptr_ = nullptr;
        Storage obj_;

        friend class FunctionRef<typename Signature::WithoutNoexcept>;

      public:
        template <typename F>
            requires std::is_function_v<F> && is_invocable_using<F>
        explicit(false) FunctionRef(F *function) noexcept
            : function_ptr_{[](const Storage fn, ParamType<Args>... args) noexcept(Noexcept) -> R
                            {
                                return get<F>(fn)(static_cast<decltype(args)>(args)...);
                            }},
              obj_{function}
        {
            assert(function != nullptr && "Must reference a function");
        }

        template <typename F, typename T = std::remove_reference_t<F>>
            requires(!is_convertible_from_specialization<std::remove_cv_t<T>> && !std::is_member_pointer_v<T> &&
                     is_invocable_using<CvRefType<T>>)
        constexpr explicit(false) FunctionRef(F &&functor) noexcept
            : function_ptr_{[](const Storage fn, ParamType<Args>... args) noexcept(Noexcept) -> R
                            {
                                CvRefType<T> obj = *get<T>(fn);
                                return obj(static_cast<decltype(args)>(args)...);
                            }},
              obj_{std::addressof(functor)}
        {
        }

        template <typename F>
            requires(NotSelf<F, FunctionRef> && is_convertible_from_specialization<F>)
        constexpr explicit(false) FunctionRef(F other) noexcept : function_ptr_{other.function_ptr_}, obj_{other.obj_}
        {
        }

        template <auto F>
            requires is_invocable_using<decltype(F)>
        constexpr explicit(false) FunctionRef(Nontype<F>) noexcept
            : function_ptr_{[](Storage, ParamType<Args>... args) noexcept(Noexcept) -> R
                            {
                                return std::invoke(F, static_cast<decltype(args)>(args)...);
                            }}
        {
            using Functor = decltype(F);
            if constexpr (std::is_pointer_v<Functor> || std::is_member_pointer_v<Functor>)
                static_assert(F != nullptr, "NTTP callable must be usable");
        }

        template <auto F, typename U, typename T = std::remove_reference_t<U>>
            requires is_invocable_using<decltype(F), CvRefType<T>> && !std::is_rvalue_reference_v<U &&>
                     constexpr FunctionRef(Nontype<F>, U &&obj) noexcept
            : function_ptr_{[](const Storage storage, ParamType<Args>... args) noexcept(Noexcept) -> R
                            {
                                CvRefType<T> self = *get<T>(storage);
                                return std::invoke(F, self, static_cast<decltype(args)>(args)...);
                            }},
        obj_{std::addressof(obj)}
        {
            using Functor = decltype(F);
            if constexpr (std::is_pointer_v<Functor> || std::is_member_pointer_v<Functor>)
                static_assert(F != nullptr, "NTTP callable must be usable");
        }

        template <auto F, typename T>
            requires is_invocable_using<decltype(F), CvType<T> *>
        constexpr FunctionRef(Nontype<F>, CvType<T> *obj) noexcept
            : function_ptr_{[](const Storage storage, ParamType<Args>... args) noexcept(Noexcept) -> R
                            {
                                return std::invoke(F, get<CvType<T>>(storage), static_cast<decltype(args)>(args)...);
                            }},
              obj_{std::addressof(obj)}
        {
            using Functor = decltype(F);
            if constexpr (std::is_pointer_v<Functor> || std::is_member_pointer_v<Functor>)
                static_assert(F != nullptr, "NTTP callable must be usable");

            if constexpr (std::is_member_pointer_v<Functor>)
                assert(obj != nullptr && "Must reference an object");
        }

        template <typename T>
            requires(!is_convertible_from_specialization<T> && !std::is_pointer_v<T> && !is_not_nontype<T>)
        FunctionRef &operator=(T) = delete;

        constexpr R operator()(Args... args) const noexcept(Noexcept)
        {
            return function_ptr_(obj_, std::forward<Args>(args)...);
        }
    };

    export template <typename F>
        requires std::is_function_v<F>
    FunctionRef(F *) -> FunctionRef<F>;

    export template <auto V>
    FunctionRef(Nontype<V>) -> FunctionRef<AdaptSignatureType<decltype(V)>>;

    export template <auto V, typename T>
    FunctionRef(Nontype<V>, T &&) -> FunctionRef<DropFirstArgToInvokeType<AdaptSignatureType<decltype(V)>, T &>>;
} // namespace retro
