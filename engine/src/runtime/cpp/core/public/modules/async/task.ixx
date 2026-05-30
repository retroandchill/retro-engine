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
import retro.core.util.deferred;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.util.exceptions;
import retro.core.async.concepts;

namespace retro
{
    template <typename T>
    concept ValidTaskResult =
        !std::same_as<T, std::monostate> && !std::same_as<T, std::exception_ptr> && !std::same_as<T, std::stop_token>;

    export template <ValidTaskResult T = void>
    class Task;

    template <typename T>
    concept PromiseLike = requires(T &promise) {
        {
            promise.perform_continuation()
        };
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
            promise.perform_continuation();
            return std::noop_coroutine();
        }

        [[noreturn]] static void await_resume() noexcept
        {
            // No resume operation
        }
    };

    struct Empty
    {
    };

    template <ValidTaskResult T>
    class TaskCompletion
    {
      public:
        explicit TaskCompletion(const bool auto_continue = false) noexcept : auto_continue_(auto_continue)
        {
        }

        bool is_complete() const noexcept
        {
            std::scoped_lock lock{mutex_};
            return !std::holds_alternative<std::monostate>(result_);
        }

        template <std::convertible_to<T> U>
        void set_result(U &&result)
            requires(!std::is_void_v<T>)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                throw InvalidStateException{"Task is already complete"};
            result_ = std::forward<U>(result);
            notify_task_complete();
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        void emplace_result(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                throw InvalidStateException{"Task is already complete"};
            result_.template emplace<T>(std::forward<Args>(args)...);
            notify_task_complete();
        }

        void set_result()
            requires(std::is_void_v<T>)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                throw InvalidStateException{"Task is already complete"};
            result_ = Empty{};
            notify_task_complete();
        }

        void set_exception(std::exception_ptr ex)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                throw InvalidStateException{"Task is already complete"};
            result_ = std::move(ex);
            notify_task_complete();
        }

        void set_cancelled(std::stop_token stop_token = {})
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                throw InvalidStateException{"Task is already complete"};
            result_ = std::move(stop_token);
            notify_task_complete();
        }

        template <std::convertible_to<T> U>
        bool try_set_result(U &&result)
            requires(!std::is_void_v<T>)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;
            result_ = std::forward<U>(result);
            notify_task_complete();
            return true;
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        bool try_emplace_result(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;
            result_.template emplace<T>(std::forward<Args>(args)...);
            notify_task_complete();
            return true;
        }

        bool try_set_result() noexcept
            requires(std::is_void_v<T>)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;
            result_ = Empty{};
            notify_task_complete();
            return true;
        }

        bool try_set_exception(std::exception_ptr ex)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;
            result_ = std::move(ex);
            notify_task_complete();
            return true;
        }

        bool try_set_cancelled(std::stop_token stop_token = {})
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;
            result_ = std::move(stop_token);
            notify_task_complete();
            return true;
        }

        T get_result()
            requires !std::is_void_v<T>
        {
            std::scoped_lock lock{mutex_};
            return get_internal();
        }

        void wait() const
        {
            std::unique_lock lock{mutex_};
            wait_internal(lock);
        }

        T wait_and_get()
        {
            std::unique_lock lock{mutex_};
            wait_internal(lock);
            return get_internal();
        }

        void set_use_captured_context(bool use_captured_context)
        {
            std::scoped_lock lock{mutex_};
            use_captured_context_ = use_captured_context;
        }

        void perform_continuation() const
        {
            std::scoped_lock lock{mutex_};
            perform_continuation_internal();
        }

        bool try_suspend(std::coroutine_handle<> continuation)
        {
            std::scoped_lock lock{mutex_};
            if (!std::holds_alternative<std::monostate>(result_))
                return false;

            continuation_ = continuation;
            captured_context_ = TaskScheduler::current();
            return true;
        }

      private:
        void wait_internal(std::unique_lock<std::mutex> &lock) const
        {
            task_complete_.wait(lock, [this] { return !std::holds_alternative<std::monostate>(result_); });
        }

        T get_internal()
        {
            return std::visit(Overload{[](std::monostate) -> T { throw InvalidStateException{"Task is empty"}; },
                                       [](T &&success) -> T { return std::move(success); },
                                       [](const std::exception_ptr &ex) -> T { std::rethrow_exception(ex); },
                                       [](std::stop_token &&stop_token) -> T
                                       {
                                           throw OperationCancelledException{std::move(stop_token)};
                                       }},
                              std::move(result_));
        }

        void notify_task_complete() const
        {
            task_complete_.notify_all();
            if (auto_continue_)
            {
                perform_continuation_internal();
            }
        }

        void perform_continuation_internal() const
        {
            if (!continuation_)
                return;

            auto &scheduler = use_captured_context_ && captured_context_.has_value()
                                  ? *captured_context_
                                  : TaskScheduler::default_scheduler();
            scheduler.enqueue(continuation_);
        }

        using SuccessValue = std::conditional_t<std::is_void_v<T>, Empty, T>;
        using ResultValue = std::variant<std::monostate, SuccessValue, std::exception_ptr, std::stop_token>;

        mutable std::mutex mutex_;
        ResultValue result_{};
        std::coroutine_handle<> continuation_;
        Optional<TaskScheduler &> captured_context_ = TaskScheduler::current();
        mutable std::condition_variable task_complete_;
        bool use_captured_context_ = true;
        bool auto_continue_ = false;
    };

    template <ValidTaskResult T>
    class TaskPromiseBase
    {
      public:
        template <typename... Args>
        explicit TaskPromiseBase(Args &&...args) : stop_token_{extract_stop_token(std::forward<Args>(args)...)}
        {
        }

        TaskPromiseBase(const TaskPromiseBase &) = delete;
        TaskPromiseBase(TaskPromiseBase &&) noexcept = delete;
        ~TaskPromiseBase() = default;
        TaskPromiseBase &operator=(const TaskPromiseBase &) = delete;
        TaskPromiseBase &operator=(TaskPromiseBase &&) noexcept = delete;

        template <typename Self>
        T get_result(this Self &&self)
            requires !std::is_void_v<T>
        {
            return self.result().get_result();
        }

        template <typename Self>
        Task<T> get_return_object(this Self &) noexcept;

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
            try
            {
                std::rethrow_exception(std::current_exception());
            }
            catch (const OperationCancelledException &ex)
            {
                if (ex.stop_token().stop_possible() && ex.stop_token() == stop_token_)
                {
                    result().set_cancelled(ex.stop_token());
                }
                else
                {
                    result().set_exception(std::current_exception());
                }
            }
            catch (...)
            {
                result_.set_exception(std::current_exception());
            }
        }

        void wait() const
        {
            result().wait();
        }

        T wait_and_get()
        {
            return result().wait_and_get();
        }

        void set_use_captured_context(bool use_captured_context)
        {
            result_.set_use_captured_context(use_captured_context);
        }

        void perform_continuation() const
        {
            result_.perform_continuation();
        }

        bool try_suspend(std::coroutine_handle<> continuation)
        {
            return result_.try_suspend(continuation);
        }

        [[nodiscard]] const std::stop_token &stop_token() const noexcept
        {
            return stop_token_;
        }

      protected:
        TaskCompletion<T> &result() noexcept
        {
            return result_;
        }

        const TaskCompletion<T> &result() const noexcept
        {
            return result_;
        }

      private:
        TaskCompletion<T> result_{};
        std::stop_token stop_token_;
    };

    template <typename T>
    class TaskPromise : public TaskPromiseBase<T>
    {
      public:
        using TaskPromiseBase<T>::TaskPromiseBase;

        template <std::convertible_to<T> U>
        void return_value(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            this->result().emplace_result(std::forward<U>(value));
        }

      private:
        friend Task<T>;
    };

    template <>
    class TaskPromise<void> : public TaskPromiseBase<void>
    {
      public:
        using TaskPromiseBase::TaskPromiseBase;

        inline void return_void() noexcept
        {
            result().set_result();
        }

      private:
        friend Task<>;
    };

    template <ValidTaskResult T>
    struct ImmediateState
    {
        using ResultType = std::variant<T, std::exception_ptr, std::stop_token>;
        ResultType result{};

        template <typename... Args>
            requires std::constructible_from<ResultType, Args...>
        explicit constexpr ImmediateState(Args &&...args) : result(std::forward<Args>(args)...)
        {
        }

        T get_result()
        {
            return std::visit(Overload{[](T &&success) -> T { return std::move(success); },
                                       [](const std::exception_ptr &ex) -> T { std::rethrow_exception(ex); },
                                       [](std::stop_token &&stop_token) -> T
                                       {
                                           throw OperationCancelledException{std::move(stop_token)};
                                       }},
                              std::move(result));
        }
    };

    template <>
    struct ImmediateState<void>
    {
        using ResultType = std::variant<std::monostate, std::exception_ptr, std::stop_token>;
        ResultType result{};

        template <typename... Args>
            requires std::constructible_from<ResultType, Args...>
        explicit constexpr ImmediateState(Args &&...args) : result(std::forward<Args>(args)...)
        {
        }

        inline void get_result()
        {
            std::visit(Overload{[](std::monostate) {},
                                [](std::exception_ptr &&ex) { std::rethrow_exception(std::move(ex)); },
                                [](std::stop_token &&stop_token)
                                {
                                    throw OperationCancelledException{std::move(stop_token)};
                                }},
                       std::move(result));
        }
    };

    export template <ValidTaskResult T>
    class TaskCompletionSource;

    template <ValidTaskResult T>
    class [[nodiscard("Tasks represent an async unit of work")]] Task
    {
        using Handle = std::coroutine_handle<TaskPromise<T>>;
        using State = std::variant<std::monostate, Handle, ImmediateState<T>, std::shared_ptr<TaskCompletion<T>>>;

        struct Awaiter
        {

            State state{};

            bool await_ready() noexcept
            {
                return std::visit(Overload{[](std::monostate) -> bool { throw InvalidStateException{"Task is empty"}; },
                                           [](const Handle &handle) -> bool { return handle.done(); },
                                           [](const ImmediateState<T> &) -> bool { return true; },
                                           [](const std::shared_ptr<TaskCompletion<T>> &completion) -> bool
                                           {
                                               return completion->is_complete();
                                           }},
                                  state);
            }

            bool await_suspend(std::coroutine_handle<> handle) noexcept
            {
                return std::visit(Overload{[](std::monostate) -> bool { throw InvalidStateException{"Task is empty"}; },
                                           [&handle](const Handle h)
                                           {
                                               if (h.done())
                                                   return false;

                                               return h.promise().try_suspend(handle);
                                           },
                                           [](const ImmediateState<T> &) { return false; },
                                           [&handle](const std::shared_ptr<TaskCompletion<T>> &completion) -> bool
                                           {
                                               return completion->try_suspend(handle);
                                           }},
                                  state);
            }

            T await_resume()
            {
                return std::visit(Overload{[](std::monostate) -> T { throw InvalidStateException{"Task is empty"}; },
                                           [](const Handle &handle) -> T
                                           {
                                               assert(handle.done());
                                               if constexpr (!std::is_void_v<T>)
                                               {
                                                   return handle.promise().get_result();
                                               }
                                               else
                                               {
                                                   return;
                                               }
                                           },
                                           [](ImmediateState<T> &&state) -> T { return state.get_result(); },
                                           [](const std::shared_ptr<TaskCompletion<T>> &completion) -> T
                                           {
                                               if constexpr (!std::is_void_v<T>)
                                               {
                                                   return completion->get_result();
                                               }
                                               else
                                               {
                                                   return;
                                               }
                                           }},
                                  std::move(state));
            }
        };

        explicit Task(Handle coro, std::stop_token stop_token) noexcept
            : state_(coro), stop_token_(std::move(stop_token))
        {
        }

        explicit Task(ImmediateState<T> state) noexcept : state_(std::move(state))
        {
        }

        explicit Task(std::shared_ptr<TaskCompletion<T>> completion) noexcept : state_(std::move(completion))
        {
        }

      public:
        Task(const Task &) = delete;
        Task(Task &&other) noexcept : state_{std::exchange(other.state_, {})}, stop_token_{std::move(other.stop_token_)}
        {
        }

        ~Task() = default;

        Task &operator=(const Task &) = delete;
        Task &operator=(Task &&other) noexcept
        {
            if (this != &other)
            {
                state_ = std::exchange(other.state_, {});
                stop_token_ = std::move(other.stop_token_);
            }
            return *this;
        }

        using promise_type = TaskPromise<T>;

        template <std::convertible_to<T> U>
            requires(!std::is_void_v<T>)
        [[nodiscard]] static Task from_result(U &&value) noexcept(std::is_nothrow_constructible_v<T, U>)
        {
            return Task{ImmediateState<T>{std::in_place_type<T>, std::forward<U>(value)}};
        }

        [[nodiscard]] static Task completed() noexcept
            requires std::is_void_v<T>
        {
            return Task{ImmediateState<T>{std::in_place_index<ImmediateState<T>::success_state>}};
        }

        [[nodiscard]] static Task from_exception(std::exception_ptr ex) noexcept
        {
            return Task{ImmediateState<T>{std::in_place_type<std::exception_ptr>, std::move(ex)}};
        }

        [[nodiscard]] static Task cancelled(std::stop_token stop_token = {}) noexcept
        {
            return Task{ImmediateState<T>{std::in_place_type<std::stop_token>, std::move(stop_token)}};
        }

        [[nodiscard]] T get() &&
            requires !std::is_void_v<T>
        {
            return std::visit(Overload{[](std::monostate) -> T { throw InvalidStateException{"Task is empty"}; },
                                       [](ImmediateState<T> &&state) -> T { return state.get_result(); },
                                       [](const Handle &handle) -> T
                                       {
                                           if (handle.done())
                                               return std::move(handle.promise().get_result());

                                           auto &promise = handle.promise();
                                           return promise.wait_and_get();
                                       },
                                       [](const std::shared_ptr<TaskCompletion<T>> &completion) -> T
                                       {
                                           return completion->wait_and_get();
                                       }},
                              std::move(state_));
        }

        void wait() const
        {
            std::visit(Overload{[](std::monostate) { throw InvalidStateException{"Task is empty"}; },
                                [](const ImmediateState<T> &)
                                {
                                    // Do nothing
                                },
                                [](const Handle &handle)
                                {
                                    if (handle.done())
                                        return;

                                    auto &promise = handle.promise();
                                    promise.wait();
                                },
                                [](const std::shared_ptr<TaskCompletion<T>> &completion)
                                {
                                    completion->wait();
                                }},
                       state_);
        }

        Awaiter operator co_await() &&
        {
            return Awaiter{std::exchange(state_, std::monostate{})};
        }

        template <typename Self>
        [[nodiscard]] decltype(auto) configure_await(this Self &&self, const bool continue_on_captured_context)
        {
            std::visit(
                Overload{
                    [](std::monostate) { throw InvalidStateException{"Task is empty"}; },
                    [](const ImmediateState<T> &)
                    {
                        // Do nothing
                    },
                    [continue_on_captured_context](const Handle &handle)
                    {
                        auto &promise = handle.promise();
                        promise.set_use_captured_context(continue_on_captured_context);
                    },
                    [continue_on_captured_context](const std::shared_ptr<TaskCompletion<T>> &completion)
                    { completion->set_use_captured_context(continue_on_captured_context); },
                },
                self.state_);

            return std::forward<Self>(self);
        }

      private:
        template <ValidTaskResult U>
        friend class TaskPromiseBase;
        friend class TaskPromise<T>;
        friend class TaskCompletionSource<T>;

        State state_{};
        std::stop_token stop_token_;
    };

    template <ValidTaskResult T>
    class TaskCompletionSource<T>
    {
      public:
        TaskCompletionSource() = default;
        TaskCompletionSource(const TaskCompletionSource &) = delete;
        TaskCompletionSource(TaskCompletionSource &&) = default;
        ~TaskCompletionSource() = default;
        TaskCompletionSource &operator=(const TaskCompletionSource &) = delete;
        TaskCompletionSource &operator=(TaskCompletionSource &&) = default;

        template <std::convertible_to<T> U>
        void set_result(U &&result)
            requires(!std::is_void_v<T>)
        {
            completion_->set_result(std::forward<U>(result));
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        void emplace_result(Args &&...args)
        {
            completion_->emplace_result(std::forward<Args>(args)...);
        }

        void set_result()
            requires(std::is_void_v<T>)
        {
            completion_->set_result();
        }

        void set_exception(std::exception_ptr ex)
        {
            completion_->set_exception(std::move(ex));
        }

        void set_cancelled(std::stop_token stop_token = {})
        {
            completion_->set_cancelled(std::move(stop_token));
        }

        template <std::convertible_to<T> U>
        bool try_set_result(U &&result)
            requires(!std::is_void_v<T>)
        {
            return completion_->try_set_result(std::forward<U>(result));
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        bool try_emplace_result(Args &&...args)
        {
            return completion_->try_emplace_result(std::forward<Args>(args)...);
        }

        bool try_set_result() noexcept
            requires(std::is_void_v<T>)
        {
            return completion_->try_set_result();
        }

        bool try_set_exception(std::exception_ptr ex)
        {
            return completion_->try_set_exception(std::move(ex));
        }

        bool try_set_cancelled(std::stop_token stop_token = {})
        {
            return completion_->try_set_cancelled(std::move(stop_token));
        }

        Task<T> get_task()
        {
            if (converted_to_task_.exchange(true))
            {
                throw InvalidStateException{"TaskCompletionSource has already been converted to a Task"};
            }

            return Task<T>{completion_};
        }

      private:
        std::shared_ptr<TaskCompletion<T>> completion_ = std::make_shared<TaskCompletion<T>>(true);
        std::atomic<bool> converted_to_task_;
    };

    export template <typename T>
        requires(!std::is_void_v<T> && std::is_constructible_v<std::remove_cvref_t<T>, T>)
    Task<std::remove_cvref_t<T>> create_task_from_result(T &&result)
    {
        return Task<std::remove_cvref_t<T>>::from_result(std::forward<T>(result));
    }

    template <ValidTaskResult T>
    template <typename Self>
    Task<T> TaskPromiseBase<T>::get_return_object(this Self &self) noexcept
    {
        return Task<T>{std::coroutine_handle<std::remove_const_t<Self>>::from_promise(self), self.stop_token()};
    }

} // namespace retro
