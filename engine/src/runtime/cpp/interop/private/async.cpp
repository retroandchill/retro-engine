/**
 * @file async.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.async.manual_task_scheduler;
import retro.core.async.task_scheduler;
import retro.interop.interop_error;

using namespace retro;

extern "C"
{
    RETRO_API ManualTaskScheduler *retro_manual_task_scheduler_create()
    {
        return new ManualTaskScheduler();
    }

    RETRO_API void retro_manual_task_scheduler_destroy(ManualTaskScheduler *scheduler)
    {
        delete scheduler;
    }

    RETRO_API void retro_manual_task_scheduler_create_scope(ManualTaskScheduler *scheduler, TaskScheduler::Scope *scope)
    {
        std::construct_at(scope, scheduler);
    }

    RETRO_API void retro_manual_task_scheduler_destroy_scope(TaskScheduler::Scope *scope)
    {
        std::destroy_at(scope);
    }

    RETRO_API void retro_manual_task_scheduler_pump_tasks(ManualTaskScheduler *scheduler,
                                                          std::int32_t max,
                                                          InteropError *error)
    {
        try_execute([&] { scheduler->pump(static_cast<std::size_t>(max)); }, *error);
    }
}
