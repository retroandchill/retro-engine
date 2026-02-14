/**
 * @file manual_task_scheduler.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.manual_task_scheduler;

namespace retro
{
    void ManualTaskScheduler::enqueue(std::coroutine_handle<> coroutine)
    {
        std::scoped_lock lock{mutex_};
        queue_.push_back(
            [h = std::move(coroutine)]
            {
                if (h && !h.done())
                    h.resume();
            });
    }

    void ManualTaskScheduler::enqueue(SimpleDelegate delegate)
    {
        std::scoped_lock lock{mutex_};
        queue_.push_back(std::move(delegate));
    }

    std::size_t ManualTaskScheduler::pump(const std::size_t max)
    {
        std::deque<SimpleDelegate> local;
        {
            std::scoped_lock lock{mutex_};
            local.swap(queue_);
        }

        std::size_t ran = 0;
        while (!local.empty() && ran < max)
        {
            auto delegate = std::move(local.front());
            local.pop_front();
            (void)delegate.execute_if_bound();
            ran++;
        }

        if (!local.empty())
        {
            std::scoped_lock lock{mutex_};

            while (!local.empty())
            {
                queue_.push_front(std::move(local.back()));
                local.pop_back();
            }
        }

        return ran;
    }
} // namespace retro
