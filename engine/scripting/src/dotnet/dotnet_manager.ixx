//
// Created by fcors on 12/23/2025.
//
module;

#include "retro/core/exports.h"
#include <coreclr_delegates.h>

export module retro.scripting.dotnet:manager;

import :loader;

namespace retro {
    export class RETRO_API DotnetManager {

    public:
        DotnetManager();

    private:
        [[nodiscard]] load_assembly_and_get_function_pointer_fn initialize_native_host() const;

        DotnetLoader loader_;
    };
}
