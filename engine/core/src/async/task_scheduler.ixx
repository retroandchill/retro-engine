/**
 * @file task_scheduler.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.async.task_scheduler;

import std;
import retro.core.functional.delegate;

namespace retro
{
    export class RETRO_API TaskScheduler
    {
      public:
        virtual ~TaskScheduler() = default;

        virtual void enqueue(std::coroutine_handle<> coroutine) = 0;
        virtual void enqueue(SimpleDelegate delegate) = 0;

        [[nodiscard]] virtual bool can_resume_inline() const noexcept
        {
            return current() == this;
        }

        static void set_current(TaskScheduler *scheduler) noexcept;
        static TaskScheduler *current() noexcept;

        class RETRO_API Scope
        {
          public:
            explicit Scope(TaskScheduler *scheduler) noexcept;
            Scope(const Scope &) = delete;
            Scope(Scope &&) noexcept = delete;

            ~Scope() noexcept;

            Scope &operator=(const Scope &) = delete;
            Scope &operator=(Scope &&) noexcept = delete;

          private:
            TaskScheduler *prev_;
        };
    };
} // namespace retro
