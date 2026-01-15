/**
 * @file script_runtime.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:interfaces.script_runtime;

import std;
import retro.core;

namespace retro
{
    export class ScriptRuntime
    {
      public:
        virtual ~ScriptRuntime() = default;

        [[nodiscard]] virtual int32 start_scripts(std::u16string_view assembly_path,
                                                  std::u16string_view class_name) const = 0;

        virtual void tick(float delta_time) = 0;

        virtual void tear_down() = 0;
    };
} // namespace retro
