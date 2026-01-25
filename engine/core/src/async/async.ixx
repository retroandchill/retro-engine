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
    export class RETRO_API TaskScheduler
    {
      public:
        virtual ~TaskScheduler() = default;

        virtual void enqueue(std::coroutine_handle<> coroutine) = 0;
        virtual void enqueue(SimpleDelegate delegate) = 0;

        static void set_current(TaskScheduler *scheduler) noexcept;
        static TaskScheduler *current() noexcept;

        class Scope
        {
          public:
            explicit Scope(TaskScheduler *scheduler) noexcept;
            Scope(const Scope &) = delete;
            Scope(Scope &&) noexcept = delete;

            ~Scope() noexcept;

            Scope &operator=(const Scope &) = delete;
            Scope &operator=(Scope &&) noexcept = delete;

          private:
            TaskScheduler *prev_;
        };
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

    template <typename T>
    concept PromiseLike = requires(T &promise) {
        {
            promise.scheduler
        } -> std::convertible_to<TaskScheduler *>;
        {
            promise.continuation
        } -> std::convertible_to<std::coroutine_handle<>>;
        {
            T::SUCCESS_STATE
        } -> std::convertible_to<usize>;
        {
            promise.index() == T::SUCCESS_STATE
        } -> std::convertible_to<bool>;
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
            if (auto continuation = promise.continuation)
            {
                if (promise.scheduler != nullptr)
                {
                    promise.scheduler->enqueue(continuation);
                }
                else
                {
                    return continuation;
                }
            }

            return std::noop_coroutine();
        }

        [[noreturn]] void await_resume() noexcept
        {
            std::terminate();
        }
    };

    template <typename T>
    struct TaskPromiseBase
    {
        static constexpr usize SUCCESS_STATE = 1;
        static constexpr usize EXCEPTION_STATE = 2;

        TaskScheduler *scheduler = TaskScheduler::current();

        std::coroutine_handle<> continuation;
        std::variant<std::monostate, T, std::exception_ptr> result;

        template <typename Self>
        Task<T> get_return_object(this Self &) noexcept;

        std::suspend_never initial_suspend() noexcept
        {
            return {};
        }

        template <typename Self>
        FinalAwaiter<Self> final_suspend(this const Self &) noexcept
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
    struct TaskPromise<void> : TaskPromiseBase<Empty>
    {
        inline void return_void() noexcept
        {
            result.emplace<SUCCESS_STATE>();
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
                return !coro || coro.done();
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
                    auto value = std::get<SUCCESS_STATE>(std::move(coro.promise().result));
                    coro.destroy();
                    return value;
                }
                else
                {
                    coro.destroy();
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
            return Awaiter{std::exchange(coro_, {})};
        }

      private:
        friend class TaskPromise<T>;
        Handle coro_;
    };

    template <typename T>
    template <typename Self>
    Task<T> TaskPromiseBase<T>::get_return_object(this Self &) noexcept
    {
        return Task<T>{std::coroutine_handle<std::remove_const_t<Self>>::from_promise(*this)};
    }
} // namespace retro
