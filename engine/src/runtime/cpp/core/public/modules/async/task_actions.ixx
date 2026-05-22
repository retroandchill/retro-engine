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
import retro.core.async.task_scheduler;
import retro.core.functional.delegate;

namespace retro
{
    export template <typename Functor, typename... Args>
        requires std::invocable<Functor, Args...>
    Task<std::invoke_result_t<Functor, Args...>> run_async(Functor &&functor, Args &&...args)
    {
        Promise<std::invoke_result_t<Functor, Args...>> promise;
        TaskScheduler::default_scheduler().enqueue(SimpleDelegate::create(
            [callback = std::forward<Functor>(functor), &promise, ... args = std::forward<Args>(args)]
            {
                if constexpr (std::is_void_v<std::invoke_result_t<Functor, Args...>>)
                {
                    std::invoke(std::forward_like<Functor>(callback), std::forward_like<Args>(args)...);
                    promise.set_value();
                }
                else
                {
                    promise.set_value(
                        std::invoke(std::forward_like<Functor>(callback), std::forward_like<Args>(args)...));
                }
            }));
        co_return co_await promise.get_future();
    }
} // namespace retro
