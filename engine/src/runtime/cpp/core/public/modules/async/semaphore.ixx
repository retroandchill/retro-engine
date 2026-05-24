/**
 * @file semaphore.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.core.async.semaphore;

import std;
import retro.core.async.task;
import retro.core.async.task_scheduler;
import retro.core.containers.optional;

namespace retro
{
    struct WaiterBase
    {
        WaiterBase *next = nullptr;

        WaiterBase() = default;
        WaiterBase(const WaiterBase &) = default;
        WaiterBase(WaiterBase &&) = default;
        virtual ~WaiterBase() = default;

        WaiterBase &operator=(const WaiterBase &) = default;
        WaiterBase &operator=(WaiterBase &&) = default;

        virtual void notify() noexcept = 0;
    };

    struct RETRO_API SyncWaiter : WaiterBase
    {
        std::mutex mutex;
        std::condition_variable cv;

        bool ready = false;

        void notify() noexcept override;
    };

    struct RETRO_API AsyncWaiter : WaiterBase
    {
        std::coroutine_handle<> continuation;
        Optional<TaskScheduler &> captured_context;
        bool continue_on_captured_context = true;

        void notify() noexcept override;
    };

    struct AcquireAwaitable;

    export class SemaphoreGuard;

    export class RETRO_API Semaphore
    {
      public:
        explicit inline Semaphore(const std::uint32_t initial, const std::uint32_t max)
            : count_{initial}, max_count_{max}
        {
            assert(initial > 0);
            assert(max >= initial);
        }

        Semaphore(const Semaphore &) = delete;
        Semaphore(Semaphore &&) = delete;

        ~Semaphore() = default;

        Semaphore &operator=(const Semaphore &) = delete;
        Semaphore &operator=(Semaphore &&) = delete;

        void acquire();
        bool try_acquire();

        AcquireAwaitable acquire_async();

        Task<SemaphoreGuard> enter_scope_async();

        void release(std::uint32_t count = 1);

      private:
        friend struct AcquireAwaitable;

        void enqueue(WaiterBase &waiter);
        Optional<WaiterBase &> dequeue();

        std::atomic<std::uint32_t> count_;
        std::uint32_t max_count_;

        WaiterBase *head_ = nullptr;
        WaiterBase *tail_ = nullptr;

        std::mutex waiters_mutex_;
    };

    struct RETRO_API AcquireAwaitable
    {
        Semaphore &semaphore;
        AsyncWaiter waiter;

        bool await_ready() const noexcept
        {
            return semaphore.try_acquire();
        }

        void await_suspend(std::coroutine_handle<> continuation) noexcept
        {
            waiter.continuation = continuation;

            std::scoped_lock lock{semaphore.waiters_mutex_};
            semaphore.enqueue(waiter);
        }

        // ReSharper disable once CppMemberFunctionMayBeStatic
        void await_resume() noexcept
        {
            // Nothing to do on resume
        }
    };

    inline AcquireAwaitable Semaphore::acquire_async()
    {
        return {.semaphore = *this};
    }

    class SemaphoreGuard
    {
      public:
        explicit inline SemaphoreGuard(Semaphore &semaphore) : semaphore_{&semaphore}
        {
            semaphore_->acquire();
        }

        inline SemaphoreGuard(Semaphore &semaphore, std::adopt_lock_t) : semaphore_{&semaphore}
        {
        }

        SemaphoreGuard(const SemaphoreGuard &) = delete;
        inline SemaphoreGuard(SemaphoreGuard &&other) noexcept : semaphore_{other.semaphore_}
        {
            other.semaphore_ = nullptr;
        }

        inline ~SemaphoreGuard()
        {
            if (semaphore_ != nullptr)
                semaphore_->release();
        }

        SemaphoreGuard &operator=(const SemaphoreGuard &) = delete;
        SemaphoreGuard &operator=(SemaphoreGuard &&) = delete;

      private:
        Semaphore *semaphore_;
    };
} // namespace retro
