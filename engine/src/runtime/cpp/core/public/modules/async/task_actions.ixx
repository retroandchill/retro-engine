/**
 * @file task_actions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.async.task_actions;

import std;
import retro.core.async.task;
import retro.core.async.task_scheduler;
import retro.core.functional.delegate;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.util.exceptions;

namespace retro
{
    namespace
    {
        struct Void
        {
        };
    } // namespace

    template <typename T>
    class AsyncRunState
    {
        using CompleteState = Void;

      public:
        constexpr bool await_ready() const noexcept
        {
            std::scoped_lock lock{mutex_};
            return result_.index() != 0;
        }

        constexpr bool await_suspend(const std::coroutine_handle<> handle) noexcept
        {
            std::scoped_lock lock{mutex_};
            {
                if (result_.index() != 0)
                    return false;
            }

            continuation_ = handle;
            return true;
        }

        constexpr T await_resume() noexcept
        {
            std::scoped_lock lock{mutex_};

            return std::visit(Overload{[](std::monostate) -> T
                                       { throw InvalidStateException{"Tried to resume incomplete async task"}; },
                                       [](CompleteState &&value) -> T
                                       {
                                           if constexpr (!std::is_void_v<T>)
                                           {
                                               return std::move(value);
                                           }
                                           else
                                           {
                                               return;
                                           }
                                       },
                                       [](const std::exception_ptr &exception) -> T
                                       {
                                           std::rethrow_exception(exception);
                                       }},
                              std::move(result_));
        }

        template <std::convertible_to<T> U>
        void set_value(U &&value)
            requires(!std::is_void_v<T>)
        {
            {
                std::scoped_lock lock{mutex_};
                result_ = std::move(value);
            }
            continuation_.resume();
        }

        void set_value() noexcept
            requires(std::is_void_v<T>)
        {
            {
                std::scoped_lock lock{mutex_};
                result_ = Void{};
            }
            continuation_.resume();
        }

      private:
        mutable std::mutex mutex_;
        std::coroutine_handle<> continuation_;
        std::variant<std::monostate, CompleteState, std::exception_ptr> result_;
    };

    export template <typename Functor, typename... Args>
        requires std::invocable<Functor, Args...>
    Task<std::invoke_result_t<Functor, Args...>> run_async(Functor &&functor, Args &&...args)
    {
        AsyncRunState<std::invoke_result_t<Functor, Args...>> promise;
        TaskScheduler::default_scheduler().enqueue(SimpleDelegate::create(
            [callback = std::forward<Functor>(functor), &promise, ... args = std::forward<Args>(args)]
            {
                if constexpr (std::is_void_v<std::invoke_result_t<Functor, Args...>>)
                {
                    std::invoke(std::forward_like<Functor>(callback), std::forward_like<Args>(args)...);
                    promise.set_value();
                }
                else
                {
                    promise.set_value(
                        std::invoke(std::forward_like<Functor>(callback), std::forward_like<Args>(args)...));
                }
            }));
        co_return co_await promise;
    }
} // namespace retro
