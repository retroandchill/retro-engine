/**
 * @file async.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core;

namespace retro
{
    void ManualTaskScheduler::enqueue(std::coroutine_handle<> coroutine)
    {
        std::scoped_lock lock{mutex_};
        queue_.push_back([h = std::move(coroutine)] { h.resume(); });
    }

    void ManualTaskScheduler::enqueue(SimpleDelegate delegate)
    {
        std::scoped_lock lock{mutex_};
        queue_.push_back(std::move(delegate));
    }

    usize ManualTaskScheduler::pump(usize max)
    {
        std::deque<SimpleDelegate> local;
        {
            std::scoped_lock lock{mutex_};
            local.swap(queue_);
        }

        usize ran = 0;
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
