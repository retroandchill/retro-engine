/**
 * @file manual_task_scheduler.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.async.manual_task_scheduler;

import retro.core.async.task_scheduler;
import std;
import retro.core.functional.delegate;

namespace retro
{
    export class RETRO_API ManualTaskScheduler final : public TaskScheduler
    {
      public:
        void enqueue(std::coroutine_handle<> coroutine) override;
        void enqueue(SimpleDelegate delegate) override;

        std::size_t pump(std::size_t max = std::dynamic_extent);

      private:
        std::mutex mutex_;
        std::deque<SimpleDelegate> queue_;
    };
} // namespace retro
