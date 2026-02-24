/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.scripting.services;

import retro.runtime.script_runtime;
import retro.scripting.backend.dotnet;

namespace retro
{
    void add_scripting_services(ServiceCollection &services)
    {
        services.add_singleton<ScriptRuntime, DotnetManager>();
    }
} // namespace retro
