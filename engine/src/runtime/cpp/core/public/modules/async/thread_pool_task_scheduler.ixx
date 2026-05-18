/**
 * @file thread_pool_task_scheduler.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.async.thread_pool_task_scheduler;

import std;
import retro.core.async.thread_pool;
import retro.core.async.task_scheduler;
import retro.core.functional.delegate;

namespace retro
{
    export class RETRO_API ThreadPoolTaskScheduler final : public TaskScheduler
    {
      public:
        static constexpr std::size_t default_thread_count = 8;

        explicit inline ThreadPoolTaskScheduler(const std::size_t thread_count = default_thread_count)
            : thread_pool_{thread_count}
        {
        }

        static ThreadPoolTaskScheduler &global_instance();

        void enqueue(std::coroutine_handle<> coroutine) override;
        void enqueue(SimpleDelegate delegate) override;

      private:
        ThreadPool thread_pool_;
    };
} // namespace retro
