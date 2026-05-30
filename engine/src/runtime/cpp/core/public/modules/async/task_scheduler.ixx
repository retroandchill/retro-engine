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
import retro.core.containers.optional;

namespace retro
{
    export class RETRO_API TaskScheduler
    {
      public:
        virtual ~TaskScheduler() = default;

        virtual void enqueue(std::coroutine_handle<> coroutine) = 0;

        inline void enqueue(SimpleDelegate delegate)
        {
            enqueue(std::move(delegate), {});
        }

        virtual void enqueue(SimpleDelegate delegate, std::stop_token stop_token) = 0;

        static void set_current(Optional<TaskScheduler &> scheduler) noexcept;
        static Optional<TaskScheduler &> current() noexcept;

        static TaskScheduler &default_scheduler();

        class RETRO_API Scope
        {
          public:
            explicit Scope(Optional<TaskScheduler &> scheduler) noexcept;
            Scope(const Scope &) = delete;
            Scope(Scope &&) noexcept = delete;

            ~Scope() noexcept;

            Scope &operator=(const Scope &) = delete;
            Scope &operator=(Scope &&) noexcept = delete;

          private:
            Optional<TaskScheduler &> prev_;
        };
    };
} // namespace retro
