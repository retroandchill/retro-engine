/**
 * @file manual_task_scheduler.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.manual_task_scheduler;
import retro.core.functional.overload;

namespace retro
{
    void ManualTaskScheduler::enqueue(std::coroutine_handle<> coroutine)
    {
        std::scoped_lock lock{mutex_};
        queue_.emplace_back(coroutine);
    }

    void ManualTaskScheduler::enqueue(SimpleDelegate delegate, std::stop_token stop_token)
    {
        std::scoped_lock lock{mutex_};
        queue_.emplace_back(std::in_place_type<DelegateCallback>, std::move(delegate), std::move(stop_token));
    }

    std::size_t ManualTaskScheduler::pump(const std::size_t max)
    {
        {
            std::scoped_lock lock{mutex_};
            pumping_.swap(queue_);
        }

        std::size_t ran = 0;
        while (!pumping_.empty() && ran < max)
        {
            try
            {
                std::visit(Overload{[](const DelegateCallback &d)
                                    {
                                        if (!d.second.stop_requested())
                                            std::ignore = d.first.execute_if_bound();
                                    },
                                    [](const std::coroutine_handle<> h)
                                    {
                                        // If we're continuing a coroutine, we want to ensure that it simply resumes in
                                        // the
                                        if (h && !h.done())
                                            h.resume();
                                    }},
                           pumping_.front());
            }
            catch (...)
            {
                // We don't want an exception to crash this loop
            }
            pumping_.pop_front();
            ran++;
        }

        if (!pumping_.empty())
        {
            std::scoped_lock lock{mutex_};

            while (!pumping_.empty())
            {
                queue_.push_front(std::move(pumping_.back()));
                pumping_.pop_back();
            }
        }

        return ran;
    }
} // namespace retro
