/**
 * @file async.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.core.async.task;

import std;
import retro.core.functional.delegate;
import retro.core.async.task_scheduler;

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

    template <Awaitable T>
    using AwaitResult = decltype(std::declval<AwaiterType<T>>().await_resume());

    export template <typename T = void>
    class Task;

    template <typename T>
    concept PromiseLike = requires(T &promise) {
        {
            promise.scheduler
        } -> std::convertible_to<TaskScheduler *>;
        {
            promise.continuation
        } -> std::convertible_to<std::coroutine_handle<>>;
    };

    template <PromiseLike Promise>
    struct FinalAwaiter
    {
        bool await_ready() noexcept
        {
            return false;
        }

        std::coroutine_handle<> await_suspend(std::coroutine_handle<Promise> handle) noexcept
        {
            auto &promise = handle.promise();
            const auto continuation = promise.continuation;
            if (!continuation)
                return std::noop_coroutine();

            if (promise.scheduler != nullptr)
            {
                if (promise.scheduler->can_resume_inline())
                    return continuation;

                promise.scheduler->enqueue(continuation);
                return std::noop_coroutine();
            }

            return continuation;
        }

        [[noreturn]] void await_resume() noexcept
        {
            // No resume operation
        }
    };

    template <typename T, typename Result = T>
    struct TaskPromiseBase
    {
        static constexpr std::size_t SUCCESS_STATE = 1;
        static constexpr std::size_t EXCEPTION_STATE = 2;

        TaskScheduler *scheduler = TaskScheduler::current();

        std::coroutine_handle<> continuation{};
        std::variant<std::monostate, T, std::exception_ptr> result{};

        template <typename Self>
        Task<Result> get_return_object(this Self &) noexcept;

        std::suspend_never initial_suspend() noexcept
        {
            return {};
        }

        template <typename Self>
        FinalAwaiter<std::decay_t<Self>> final_suspend(this Self &&) noexcept
        {
            return {};
        }

        void unhandled_exception() noexcept
        {
            result.template emplace<EXCEPTION_STATE>(std::current_exception());
        }
    };

    template <typename T>
    struct TaskPromise : TaskPromiseBase<T>
    {
        template <std::convertible_to<T> U>
        void return_value(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            this->result.template emplace<TaskPromiseBase<T>::SUCCESS_STATE>(std::forward<U>(value));
        }
    };

    struct Empty
    {
    };

    template <>
    struct TaskPromise<void> : TaskPromiseBase<Empty, void>
    {
        inline void return_void() noexcept
        {
            result.emplace<SUCCESS_STATE>();
        }
    };

    template <typename T>
    struct ImmediateState
    {
        using ResultType = std::variant<std::monostate, T, std::exception_ptr>;
        ResultType result{};

        template <typename... Args>
            requires std::constructible_from<ResultType, Args...>
        explicit constexpr ImmediateState(Args &&...args) : result(std::forward<Args>(args)...)
        {
        }

        static constexpr std::size_t SUCCESS_STATE = 1;
        static constexpr std::size_t EXCEPTION_STATE = 2;
    };

    template <>
    struct ImmediateState<void>
    {
        using ResultType = std::variant<std::monostate, std::exception_ptr>;
        ResultType result{};

        template <typename... Args>
            requires std::constructible_from<ResultType, Args...>
        explicit constexpr ImmediateState(Args &&...args) : result(std::forward<Args>(args)...)
        {
        }

        // monostate => success for void
        static constexpr std::size_t SUCCESS_STATE = 0;
        static constexpr std::size_t EXCEPTION_STATE = 1;
    };

    template <typename T>
    class [[nodiscard]] Task
    {
        using Handle = std::coroutine_handle<TaskPromise<T>>;
        using State = std::variant<std::monostate, Handle, ImmediateState<T>>;

        struct Awaiter
        {
            static constexpr auto SUCCESS_STATE = TaskPromise<T>::SUCCESS_STATE;
            static constexpr auto EXCEPTION_STATE = TaskPromise<T>::EXCEPTION_STATE;

            State state{};

            bool await_ready() noexcept
            {
                if (std::holds_alternative<ImmediateState<T>>(state))
                    return true;

                if (auto *h = std::get_if<Handle>(&state))
                    return !*h || h->done();

                return true;
            }

            void await_suspend(std::coroutine_handle<> handle) noexcept
            {
                auto *h = std::get_if<Handle>(&state);
                if (!h || !*h)
                    return;

                h->promise().continuation = handle;
            }

            T await_resume()
            {
                if (auto *imm = std::get_if<ImmediateState<T>>(&state))
                {
                    if (imm->result.index() == ImmediateState<T>::EXCEPTION_STATE)
                        std::rethrow_exception(std::get<ImmediateState<T>::EXCEPTION_STATE>(imm->result));

                    assert(imm->result.index() == ImmediateState<T>::SUCCESS_STATE);

                    if constexpr (!std::is_void_v<T>)
                        return std::get<ImmediateState<T>::SUCCESS_STATE>(std::move(imm->result));
                    else
                        return;
                }

                auto &coro = std::get<Handle>(state);

                if (coro.promise().result.index() == EXCEPTION_STATE)
                    std::rethrow_exception(std::get<EXCEPTION_STATE>(coro.promise().result));

                assert(coro.promise().result.index() == SUCCESS_STATE);

                if constexpr (!std::is_void_v<T>)
                {
                    auto value = std::get<SUCCESS_STATE>(std::move(coro.promise().result));
                    coro.destroy();
                    return value;
                }
                else
                {
                    coro.destroy();
                    return;
                }
            }
        };

        explicit Task(Handle coro) noexcept : state_(coro)
        {
        }

        explicit Task(ImmediateState<T> state) noexcept : state_(std::move(state))
        {
        }

      public:
        using promise_type = TaskPromise<T>;

        Task(const Task &) = delete;
        Task(Task &&other) noexcept : state_{std::exchange(other.state_, {})}
        {
        }

        ~Task() noexcept
        {
            if (auto *coro = std::get_if<Handle>(&state_); coro != nullptr && *coro)
                coro->destroy();
        }

        Task &operator=(const Task &) = delete;
        Task &operator=(Task &&other) noexcept
        {
            if (auto *coro = std::get_if<Handle>(&state_); coro != nullptr && *coro)
                coro->destroy();
            state_ = std::exchange(other.state_, {});
            return *this;
        }

        template <std::convertible_to<T> U>
            requires(!std::is_void_v<T>)
        [[nodiscard]] static Task from_result(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            return Task{
                ImmediateState<T>{std::in_place_index<ImmediateState<T>::SUCCESS_STATE>, std::forward<U>(value)}};
        }

        [[nodiscard]] static Task completed() noexcept
            requires std::is_void_v<T>
        {
            return Task{ImmediateState<T>{std::in_place_index<ImmediateState<T>::SUCCESS_STATE>}};
        }

        [[nodiscard]] static Task from_exception(std::exception_ptr ex) noexcept
        {
            return Task{ImmediateState<T>{std::in_place_index<ImmediateState<T>::EXCEPTION_STATE>, std::move(ex)}};
        }

        Awaiter operator co_await() &&
        {
            return Awaiter{std::exchange(state_, {})};
        }

      private:
        template <typename U, typename Result>
        friend struct TaskPromiseBase;
        friend class TaskPromise<T>;

        State state_{};
    };

    template <typename T, typename Result>
    template <typename Self>
    Task<Result> TaskPromiseBase<T, Result>::get_return_object(this Self &self) noexcept
    {
        return Task<Result>{std::coroutine_handle<std::remove_const_t<Self>>::from_promise(self)};
    }
} // namespace retro
