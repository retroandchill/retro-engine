/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.async.concepts;

import std;
import retro.core.type_traits.arguments;

namespace retro
{
    template <typename T>
    concept Awaiter = requires(T &x) {
        {
            x.await_ready()
        } -> std::convertible_to<bool>;
        x.await_resume();
    };

    template <typename T>
    concept MemberCoAwait = requires(T &&x) {
        {
            std::forward<T>(x).operator co_await()
        } -> Awaiter;
    };

    template <typename T>
    concept FreeCoAwait = requires(T &&x) {
        {
            operator co_await(std::forward<T>(x))
        } -> Awaiter;
    };

    export template <typename T>
    concept Awaitable = MemberCoAwait<T> || FreeCoAwait<T> || Awaiter<T>;

    template <MemberCoAwait T>
    decltype(auto) get_awaiter(T &&x) noexcept(noexcept(std::forward<T>(x).operator co_await()))
    {
        return std::forward<T>(x).operator co_await();
    }

    template <FreeCoAwait T>
    decltype(auto) get_awaiter(T &&x) noexcept(noexcept(operator co_await(std::forward<T>(x))))
    {
        return operator co_await(std::forward<T>(x));
    }

    template <Awaiter T>
        requires(!MemberCoAwait<T> && !FreeCoAwait<T>)
    T &&get_awaiter(T &&x) noexcept
    {
        return std::forward<T>(x);
    }

    template <Awaitable T>
    using AwaiterType = decltype(get_awaiter(std::declval<T>()));

    export template <Awaitable T>
    using AwaitResult = decltype(std::declval<AwaiterType<T>>().await_resume());

    export template <typename T>
    concept StopTokenLike = std::same_as<std::remove_cvref_t<T>, std::stop_token>;

    export template <typename... Args>
    concept FirstArgumentIsStopToken = HasArguments<Args...> && StopTokenLike<FirstArgumentType<Args...>>;

    export template <typename... Args>
    concept LastArgumentIsStopToken = HasArguments<Args...> && StopTokenLike<LastArgumentType<Args...>>;

    template <typename... Args>
    struct CoroutineStopTokenTraits
    {
        static constexpr bool has_arguments = false;
        static constexpr bool first_is_stop_token = false;
        static constexpr bool last_is_stop_token = false;
        static constexpr bool has_stop_token = false;
        static constexpr bool ambiguous = false;

        static std::stop_token extract(Args &&...) noexcept
        {
            return {};
        }
    };

    template <typename First, typename... Rest>
    struct CoroutineStopTokenTraits<First, Rest...>
    {
      private:
        using Last = LastArgumentType<First, Rest...>;

      public:
        static constexpr bool has_arguments = true;
        static constexpr bool first_is_stop_token = StopTokenLike<First>;
        static constexpr bool last_is_stop_token = StopTokenLike<Last>;

        static constexpr bool single_argument = sizeof...(Rest) == 0;

        static constexpr bool has_stop_token = first_is_stop_token || last_is_stop_token;

        static constexpr bool ambiguous = first_is_stop_token && last_is_stop_token && !single_argument;

        static std::stop_token extract(First &&first, Rest &&...rest) noexcept
        {
            static_assert(!ambiguous,
                          "Task coroutine cancellation token is ambiguous. "
                          "Use either a leading std::stop_token or a trailing std::stop_token, not both.");

            if constexpr (last_is_stop_token)
            {
                return extract_last(std::forward<First>(first), std::forward<Rest>(rest)...);
            }
            else if constexpr (first_is_stop_token)
            {
                return first;
            }
            else
            {
                return {};
            }
        }

      private:
        static std::stop_token extract_last(Last &&last) noexcept
        {
            return last;
        }

        template <typename Current, typename... Tail>
        static std::stop_token extract_last(Current &&, Tail &&...tail) noexcept
        {
            return extract_last(std::forward<Tail>(tail)...);
        }
    };

    export template <typename... Args>
    std::stop_token extract_stop_token(Args &&...args) noexcept
    {
        return CoroutineStopTokenTraits<Args...>::extract(std::forward<Args>(args)...);
    }

    template <typename, typename...>
    struct InvocableStopTokenTraits
    {
        static constexpr bool is_callable = false;
    };

    template <typename Functor, typename... Args>
        requires std::invocable<Functor, Args...>
    struct InvocableStopTokenTraits<Functor, Args...>
    {
        using ReturnType = std::invoke_result_t<Functor, Args...>;
        static constexpr bool is_callable = true;
        static constexpr bool is_noexcept = std::is_nothrow_invocable_v<Functor, Args...>;

        static decltype(auto) call_on_args(Functor &&functor, Args &&...args) noexcept(is_noexcept)
        {
            return std::invoke(std::forward<Functor>(functor), std::forward<Args>(args)...);
        }
    };

    template <typename Functor, typename... Args>
        requires FirstArgumentIsStopToken<Args...> && InvocableWithoutFirstArg<Functor, Args...>
    struct InvocableStopTokenTraits<Functor, Args...>
    {
        using ReturnType = InvokeWithoutFirstArgResult<Functor, Args...>;
        static constexpr bool is_callable = true;
        static constexpr bool is_noexcept = NothrowInvocableWithoutFirstArg<Functor, Args...>;

        static decltype(auto) call_on_args(Functor &&functor, Args &&...args) noexcept(is_noexcept)
        {
            return invoke_without_first_arg(std::forward<Functor>(functor), std::forward<Args>(args)...);
        }
    };

    template <typename Functor, typename... Args>
        requires LastArgumentIsStopToken<Args...> && InvocableWithoutLastArg<Functor, Args...>
    struct InvocableStopTokenTraits<Functor, Args...>
    {
        using ReturnType = InvokeWithoutLastArgResult<Functor, Args...>;
        static constexpr bool is_callable = true;
        static constexpr bool is_noexcept = NothrowInvocableWithoutLastArg<Functor, Args...>;

        static decltype(auto) call_on_args(Functor &&functor, Args &&...args) noexcept(is_noexcept)
        {
            return invoke_without_last_arg(std::forward<Functor>(functor), std::forward<Args>(args)...);
        }
    };

    export template <typename Functor, typename... Args>
    concept InvocableWithOptionalStopToken = InvocableStopTokenTraits<Functor, Args...>::is_callable;

    export template <typename Functor, typename... Args>
    concept NothrowInvocableWithOptionalStopToken =
        InvocableWithOptionalStopToken<Functor, Args...> && InvocableStopTokenTraits<Functor, Args...>::is_noexcept;

    export template <typename Functor, typename... Args>
        requires InvocableWithOptionalStopToken<Functor, Args...>
    using InvocableWithOptionalStopTokenResult = InvocableStopTokenTraits<Functor, Args...>::ReturnType;

    export template <typename Functor, typename... Args>
        requires InvocableWithOptionalStopToken<Functor, Args...>
    decltype(auto) invoke_with_optional_stop_token(Functor &&functor, Args &&...args) noexcept(
        NothrowInvocableWithOptionalStopToken<Functor, Args...>)
    {
        return InvocableStopTokenTraits<Functor, Args...>::call_on_args(std::forward<Functor>(functor),
                                                                        std::forward<Args>(args)...);
    }

} // namespace retro
