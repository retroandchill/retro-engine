/**
 * @file semaphore.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.core.async.semaphore;

import retro.core.util.exceptions;

namespace retro
{
    void SyncWaiter::notify() noexcept
    {
        {
            std::scoped_lock lock{mutex};
            ready = true;
        }

        cv.notify_one();
    }

    void AsyncWaiter::notify() noexcept
    {
        if (continue_on_captured_context && captured_context.has_value())
        {
            captured_context->enqueue(continuation);
        }
        else
        {
            TaskScheduler::default_scheduler().enqueue(continuation);
        }
    }

    void Semaphore::acquire()
    {
        if (try_acquire())
            return;

        SyncWaiter waiter;

        {
            std::scoped_lock lock{waiters_mutex_};
            enqueue(waiter);
        }

        std::unique_lock lock{waiter.mutex};
        waiter.cv.wait(lock, [&waiter] { return waiter.ready; });
    }

    bool Semaphore::try_acquire()
    {
        auto old = count_.load(std::memory_order_relaxed);

        while (old > 0)
        {
            if (count_.compare_exchange_weak(old, old - 1, std::memory_order_acquire))
                return true;
        }

        return false;
    }

    Task<SemaphoreGuard> Semaphore::enter_scope_async()
    {
        co_await acquire_async();
        co_return SemaphoreGuard{*this, std::adopt_lock};
    }

    void Semaphore::release(const std::uint32_t count)
    {
        if (count_ + count > max_count_)
            throw InvalidStateException{"Semaphore count exceeded"};

        for (std::uint32_t i = 0; i < count; ++i)
        {
            Optional<WaiterBase &> waiter;

            {
                std::scoped_lock lock{waiters_mutex_};

                waiter = dequeue();

                if (!waiter.has_value())
                {
                    ++count_;
                    continue;
                }
            }

            waiter->notify();
        }
    }

    void Semaphore::enqueue(WaiterBase &waiter)
    {
        if (head_ == nullptr)
        {
            assert(tail_ == nullptr);
            head_ = &waiter;
            tail_ = &waiter;
        }
        else
        {
            assert(tail_ != nullptr);
            tail_->next = &waiter;
            tail_ = &waiter;
        }
    }

    Optional<WaiterBase &> Semaphore::dequeue()
    {
        if (head_ == nullptr)
        {
            assert(tail_ == nullptr);
            return std::nullopt;
        }

        assert(tail_ != nullptr);
        auto *waiter = head_;
        head_ = waiter->next;
        if (head_ == nullptr)
        {
            tail_ = nullptr;
        }
        return waiter;
    }
} // namespace retro
