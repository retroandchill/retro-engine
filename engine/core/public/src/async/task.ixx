/**
 * @file task.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

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
        static bool await_ready() noexcept
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

        [[noreturn]] static void await_resume() noexcept
        {
            // No resume operation
        }
    };

    template <typename T, typename Result = T>
    struct TaskPromiseBase
    {
        static constexpr std::size_t success_state = 1;
        static constexpr std::size_t exception_state = 2;

        TaskScheduler *scheduler = TaskScheduler::current();

        std::coroutine_handle<> continuation{};
        std::variant<std::monostate, T, std::exception_ptr> result{};

        template <typename Self>
        Task<Result> get_return_object(this Self &) noexcept;

        static std::suspend_never initial_suspend() noexcept
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
            result.template emplace<exception_state>(std::current_exception());
        }
    };

    template <typename T>
    struct TaskPromise : TaskPromiseBase<T>
    {
        template <std::convertible_to<T> U>
        void return_value(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            this->result.template emplace<TaskPromiseBase<T>::success_state>(std::forward<U>(value));
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
            result.emplace<success_state>();
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

        static constexpr std::size_t success_state = 1;
        static constexpr std::size_t exception_state = 2;
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
        static constexpr std::size_t success_state = 0;
        static constexpr std::size_t exception_state = 1;
    };

    template <typename T>
    class [[nodiscard("Tasks represent an async unit of work")]] Task
    {
        using Handle = std::coroutine_handle<TaskPromise<T>>;
        using State = std::variant<std::monostate, Handle, ImmediateState<T>>;

        struct Awaiter
        {
            static constexpr auto success_state = TaskPromise<T>::success_state;
            static constexpr auto exception_state = TaskPromise<T>::exception_state;

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
                    if (imm->result.index() == ImmediateState<T>::exception_state)
                        std::rethrow_exception(std::get<ImmediateState<T>::exception_state>(imm->result));

                    assert(imm->result.index() == ImmediateState<T>::success_state);

                    if constexpr (!std::is_void_v<T>)
                        return std::get<ImmediateState<T>::success_state>(std::move(imm->result));
                    else
                        return;
                }

                auto &coro = std::get<Handle>(state);

                if (coro.promise().result.index() == exception_state)
                    std::rethrow_exception(std::get<exception_state>(coro.promise().result));

                assert(coro.promise().result.index() == success_state);

                if constexpr (!std::is_void_v<T>)
                {
                    auto value = std::get<success_state>(std::move(coro.promise().result));
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
                ImmediateState<T>{std::in_place_index<ImmediateState<T>::success_state>, std::forward<U>(value)}};
        }

        [[nodiscard]] static Task completed() noexcept
            requires std::is_void_v<T>
        {
            return Task{ImmediateState<T>{std::in_place_index<ImmediateState<T>::success_state>}};
        }

        [[nodiscard]] static Task from_exception(std::exception_ptr ex) noexcept
        {
            return Task{ImmediateState<T>{std::in_place_index<ImmediateState<T>::exception_state>, std::move(ex)}};
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
