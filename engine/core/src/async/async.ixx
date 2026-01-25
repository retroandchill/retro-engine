/**
 * @file async.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.core:async;

import std;
import :functional;
import :defines;

namespace retro
{
    export class TaskScheduler
    {
      public:
        virtual ~TaskScheduler() = default;

        virtual void enqueue(std::coroutine_handle<> coroutine) = 0;
        virtual void enqueue(SimpleDelegate delegate) = 0;
    };

    export class RETRO_API ManualTaskScheduler final : public TaskScheduler
    {
      public:
        void enqueue(std::coroutine_handle<> coroutine) override;
        void enqueue(SimpleDelegate delegate) override;

        usize pump(usize max = std::dynamic_extent);

      private:
        std::mutex mutex_;
        std::deque<SimpleDelegate> queue_;
    };

    export template <typename T>
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

    using TaskInitialSuspendType = std::suspend_always;

    template <typename T>
    struct TaskPromise
    {
        static constexpr usize SUCCESS_STATE = 1;
        static constexpr usize EXCEPTION_STATE = 2;

        std::coroutine_handle<> continuation;
        std::variant<std::monostate, T, std::exception_ptr> result;

        Task<T> get_return_object() noexcept;

        TaskInitialSuspendType initial_suspend() noexcept;

        struct FinalAwaiter
        {
            bool await_ready() noexcept
            {
                return false;
            }

            std::coroutine_handle<> await_suspend(std::coroutine_handle<TaskPromise> handle) noexcept
            {
                return handle.promise().continuation;
            }

            [[noreturn]] void await_resume() noexcept
            {
                std::terminate();
            }
        };

        FinalAwaiter final_suspend() noexcept
        {
            return {};
        }

        template <std::convertible_to<T> U>
        void return_value(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            result.template emplace<SUCCESS_STATE>(std::forward<U>(value));
        }

        void unhandled_exception() noexcept
        {
            result.template emplace<EXCEPTION_STATE>(std::current_exception());
        }
    };

    template <>
    struct TaskPromise<void>
    {
        static constexpr usize SUCCESS_STATE = 1;
        static constexpr usize EXCEPTION_STATE = 2;

        struct Empty
        {
        };

        std::coroutine_handle<> continuation;
        std::variant<std::monostate, Empty, std::exception_ptr> result;

        Task<> get_return_object() noexcept;

        inline TaskInitialSuspendType initial_suspend() noexcept
        {
            return {};
        }

        struct FinalAwaiter
        {
            bool await_ready() noexcept
            {
                return false;
            }

            std::coroutine_handle<> await_suspend(std::coroutine_handle<TaskPromise> handle) noexcept
            {
                return handle.promise().continuation;
            }

            [[noreturn]] void await_resume() noexcept
            {
                std::terminate();
            }
        };

        FinalAwaiter final_suspend() noexcept
        {
            return {};
        }

        void return_void() noexcept
        {
            result.emplace<SUCCESS_STATE>();
        }

        void unhandled_exception() noexcept
        {
            result.emplace<EXCEPTION_STATE>(std::current_exception());
        }
    };

    template <typename T>
    class [[nodiscard]] Task
    {
        using Handle = std::coroutine_handle<TaskPromise<T>>;

        struct Awaiter
        {
            static constexpr auto SUCCESS_STATE = TaskPromise<T>::SUCCESS_STATE;
            static constexpr auto EXCEPTION_STATE = TaskPromise<T>::EXCEPTION_STATE;

            Handle coro;

            bool await_ready() noexcept
            {
                return false;
            }

            Handle await_suspend(std::coroutine_handle<> handle) noexcept
            {
                coro.promise().continuation = handle;
                return coro;
            }

            T await_resume()
            {
                if (coro.promise().result.index() == EXCEPTION_STATE)
                {
                    std::rethrow_exception(std::get<EXCEPTION_STATE>(coro.promise().result));
                }

                assert(coro.promise().result.index() == SUCCESS_STATE);

                if constexpr (!std::is_void_v<T>)
                {
                    return std::get<SUCCESS_STATE>(std::move(coro.promise().result));
                }
            }
        };

        explicit Task(Handle coro) noexcept : coro_(coro)
        {
        }

      public:
        using promise_type = TaskPromise<T>;

        Task(const Task &) = delete;
        Task(Task &&other) noexcept : coro_{std::exchange(other.coro_, {})}
        {
        }

        ~Task() noexcept
        {
            if (coro_)
                coro_.destroy();
        }

        Task &operator=(const Task &) = delete;
        Task &operator=(Task &&other) noexcept
        {
            if (coro_)
                coro_.destroy();
            coro_ = std::exchange(other.coro_, {});
            return *this;
        }

        Awaiter operator co_await() &&
        {
            return Awaiter{coro_};
        }

      private:
        friend class TaskPromise<T>;
        Handle coro_;
    };

    template <typename T>
    Task<T> TaskPromise<T>::get_return_object() noexcept
    {
        return Task<T>{std::coroutine_handle<TaskPromise>::from_promise(*this)};
    }

    inline Task<> TaskPromise<void>::get_return_object() noexcept
    {
        return Task{std::coroutine_handle<TaskPromise>::from_promise(*this)};
    }
} // namespace retro
