/**
 * @file task_actions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.async.task_actions;

import std;
import retro.core.async.task;
import retro.core.async.task_scheduler;
import retro.core.functional.delegate;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.util.exceptions;
import retro.core.async.concepts;

namespace retro
{
    template <typename Functor, typename... Args>
    struct InvocableOnAllButLast;

    export template <typename Functor, typename... Args>
        requires InvocableWithOptionalStopToken<Functor, Args...>
    Task<InvocableWithOptionalStopTokenResult<Functor, Args...>> run_async(Functor &&functor, Args &&...args)
    {
        TaskCompletionSource<InvocableWithOptionalStopTokenResult<Functor, Args...>> promise;
        auto stop_token = extract_stop_token(std::forward<Args>(args)...);
        TaskScheduler::default_scheduler().enqueue(
            SimpleDelegate::create(
                [callback = std::forward<Functor>(functor), &promise, ... args = std::forward<Args>(args)]
                {
                    try
                    {
                        if constexpr (std::is_void_v<InvocableWithOptionalStopTokenResult<Functor, Args...>>)
                        {
                            invoke_with_optional_stop_token(std::forward_like<Functor>(callback),
                                                            std::forward_like<Args>(args)...);
                            promise.set_result();
                        }
                        else
                        {
                            promise.set_result(invoke_with_optional_stop_token(std::forward_like<Functor>(callback),
                                                                               std::forward_like<Args>(args)...));
                        }
                    }
                    catch (...)
                    {
                        promise.set_exception(std::current_exception());
                    }
                }),
            std::move(stop_token));
        co_return co_await promise.get_task();
    }
} // namespace retro
