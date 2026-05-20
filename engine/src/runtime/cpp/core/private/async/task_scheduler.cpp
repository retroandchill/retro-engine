/**
 * @file task_scheduler.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.task_scheduler;

import retro.core.async.thread_pool_task_scheduler;

namespace retro
{
    namespace
    {
        thread_local Optional<TaskScheduler &> current_scheduler = std::nullopt;
        ;
    } // namespace

    void TaskScheduler::set_current(const Optional<TaskScheduler &> scheduler) noexcept
    {
        current_scheduler = scheduler;
    }

    Optional<TaskScheduler &> TaskScheduler::current() noexcept
    {
        return current_scheduler;
    }
    TaskScheduler &TaskScheduler::default_scheduler()
    {
        static ThreadPoolTaskScheduler singleton;
        return singleton;
    }

    TaskScheduler::Scope::Scope(Optional<TaskScheduler &> scheduler) noexcept : prev_{current_scheduler}
    {
        current_scheduler = scheduler;
    }

    TaskScheduler::Scope::~Scope() noexcept
    {
        current_scheduler = prev_;
    }
} // namespace retro
