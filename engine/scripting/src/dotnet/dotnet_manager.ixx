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
    export class RETRO_API DotnetManager final : public ScriptRuntime
    {

      public:
        DotnetManager();

      private:
        [[nodiscard]] load_assembly_and_get_function_pointer_fn initialize_native_host() const;

        DotnetLoader loader_;
    };
} // namespace retro
