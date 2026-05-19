/**
 * @file task_actions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.async.task_actions;

import std;
import retro.core.async.future;
import retro.core.async.task;
import retro.core.async.thread_pool_task_scheduler;

namespace retro
{
    export template <std::invocable Functor>
    Task<std::invoke_result_t<Functor>> run_as_task(Functor &&functor)
    {
        Promise<std::invoke_result_t<Functor>()> promise;
        ThreadPoolTaskScheduler::global_instance().enqueue(
            [callback = std::forward<Functor>(functor), &promise]
            {
                if constexpr (std::is_void_v<std::invoke_result_t<Functor>>)
                {
                    std::invoke(callback);
                    promise.set_value();
                }
                else
                {
                    promise.set_value(std::invoke(callback));
                }
            });
        co_return co_await promise.get_future();
    }
} // namespace retro
