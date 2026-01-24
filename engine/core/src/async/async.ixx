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

    export template <typename T = void>
    class Task;

    template <typename T>
    struct TaskPromise;

    template <typename T>
    class Task
    {
      public:
        using promise_type = TaskPromise<T>;

        Task() = default;

        Task(const Task &) = delete;
        Task(Task &&other) noexcept = default;

        ~Task() = default;

        Task &operator=(const Task &) = delete;

        Task &operator=(Task &&other) noexcept = default;

      private:
        using HandleType = std::coroutine_handle<promise_type>;
        explicit Task(HandleType handle) noexcept : handle_{handle}
        {
        }

        HandleType handle_{};

        template <typename U>
        friend struct TaskPromise;
    };

    struct FinalAwaiter
    {
        bool await_ready() noexcept
        {
            return false;
        }

        template <typename Promise>
        void await_suspend(std::coroutine_handle<Promise> handle) noexcept
        {
            if (auto c = handle.promise().continuation_; c)
                c.resume();
        }

        void await_resume() noexcept
        {
        }
    };

    struct TaskPromiseBase
    {
        template <typename Self>
        Task<Self> get_return_object(Self &) noexcept
        {
            using HandleType = std::coroutine_handle<Self>;
            return Task<Self>{HandleType::from_promise(*this)};
        }

        inline std::suspend_never initial_suspend() noexcept
        {
            return {};
        }

        inline FinalAwaiter final_suspend() noexcept
        {
            return {};
        }
    };

    template <typename T>
    struct TaskPromise : TaskPromiseBase
    {
        struct NotReady
        {
        };

        std::variant<NotReady, T, std::exception_ptr> result_{};
        std::coroutine_handle<> continuation_{};

        void unhandled_exception() noexcept
        {
            result_.template emplace<std::exception_ptr>(std::current_exception());
        }

        template <typename U>
            requires std::convertible_to<U, T>
        void return_value(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            return result_.template emplace<T>(std::forward<U>(value));
        }

        T take_result()
        {
            if (const auto *ep = std::get_if<std::exception_ptr>(&result_); ep != nullptr)
            {
                std::rethrow_exception(ep);
            }

            return std::get<T>(std::move(result_));
        }
    };

    template <>
    struct TaskPromise<void> : TaskPromiseBase
    {
        struct NotReady
        {
        };
        struct Done
        {
        };

        std::variant<NotReady, Done, std::exception_ptr> result_{};
        std::coroutine_handle<> continuation_{};

        inline void unhandled_exception() noexcept
        {
            result_.emplace<std::exception_ptr>(std::current_exception());
        }

        inline void return_void() noexcept
        {
            result_.emplace<Done>();
        }

        inline void rethrow_if_failed()
        {
            if (const auto *ep = std::get_if<std::exception_ptr>(&result_); ep != nullptr)
                std::rethrow_exception(*ep);

            assert(std::holds_alternative<Done>(result_));
        }
    };
} // namespace retro
