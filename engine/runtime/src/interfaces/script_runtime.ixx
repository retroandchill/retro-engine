/**
 * @file script_runtime.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:interfaces.script_runtime;

namespace retro
{
    export class ScriptRuntime
    {
      public:
        virtual ~ScriptRuntime() = default;
    };
} // namespace retro
