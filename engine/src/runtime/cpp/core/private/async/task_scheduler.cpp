/**
 * @file task_scheduler.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.task_scheduler;

namespace retro
{
    namespace
    {
        thread_local TaskScheduler *current_scheduler = nullptr;
    }

    void TaskScheduler::set_current(TaskScheduler *scheduler) noexcept
    {
        current_scheduler = scheduler;
    }

    TaskScheduler *TaskScheduler::current() noexcept
    {
        return current_scheduler;
    }

    TaskScheduler::Scope::Scope(TaskScheduler *scheduler) noexcept : prev_{current_scheduler}
    {
        current_scheduler = scheduler;
    }

    TaskScheduler::Scope::~Scope() noexcept
    {
        current_scheduler = prev_;
    }
} // namespace retro
