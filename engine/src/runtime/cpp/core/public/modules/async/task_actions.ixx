/**
 * @file task_actions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.async.task_actions;

import std;
import retro.core.async.task;

namespace retro
{
    export template <std::invocable Functor>
    Task<std::invoke_result_t<Functor>> run_as_task(Functor &&functor)
    {
    }
} // namespace retro
