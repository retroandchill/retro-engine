/**
 * @file dotnet_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <coreclr_delegates.h>

export module retro.scripting:dotnet.manager;

import retro.runtime;
import :dotnet.loader;

namespace retro
{
    using StartFn = int32(_cdecl *)(const char16_t *assembly_path,
                                    int32 assembly_path_length,
                                    const char16_t *class_name,
                                    int32 class_name_length);
    using TickFn = int32(_cdecl *)(float delta_time, int32 max_tasks);
    using ExitFn = void(_cdecl *)();

    export struct ScriptingCallbacks
    {
        StartFn start = nullptr;
        TickFn tick = nullptr;
        ExitFn exit = nullptr;
    };

    export class RETRO_API DotnetManager final : public ScriptRuntime
    {

      public:
        DotnetManager();

        [[nodiscard]] int32 start_scripts(std::u16string_view assembly_path,
                                          std::u16string_view class_name) const override;

        void tick(float delta_time) override;

        void tear_down() override;

      private:
        [[nodiscard]] load_assembly_and_get_function_pointer_fn initialize_native_host() const;

        DotnetLoader loader_;
        ScriptingCallbacks callbacks_;
    };
} // namespace retro
