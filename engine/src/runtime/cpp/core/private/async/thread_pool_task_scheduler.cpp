/**
 * @file thread_pool_task_scheduler.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.thread_pool_task_scheduler;

namespace retro
{

    ThreadPoolTaskScheduler &ThreadPoolTaskScheduler::global_instance()
    {
        static ThreadPoolTaskScheduler singleton;
        return singleton;
    }

    void ThreadPoolTaskScheduler::enqueue(std::coroutine_handle<> coroutine)
    {
        thread_pool_.post(
            [coroutine]
            {
                if (coroutine && !coroutine.done())
                    coroutine.resume();
            });
    }

    void ThreadPoolTaskScheduler::enqueue(SimpleDelegate delegate)
    {
        thread_pool_.post([delegate = std::move(delegate)] { std::ignore = delegate.execute_if_bound(); });
    }
} // namespace retro
