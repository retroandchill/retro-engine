//
// Created by fcors on 12/23/2025.
//
module;

#include "retro/core/exports.h"

#include <hostfxr.h>

export module retro.scripting.dotnet:host;

import std;

namespace retro::scripting {
    export class RETRO_API DotnetHost {
        // ReSharper disable once CppParameterMayBeConst
        inline explicit DotnetHost(hostfxr_handle handle) : handle_{handle} {}

    public:
        ~DotnetHost();

        DotnetHost(const DotnetHost&) = delete;
        DotnetHost& operator=(const DotnetHost&) = delete;
        DotnetHost(DotnetHost&&) noexcept = delete;
        DotnetHost& operator=(DotnetHost&&) noexcept = delete;

        [[nodiscard]] inline hostfxr_handle handle() const noexcept { return handle_; }

    private:
        friend class DotnetLoader;

        hostfxr_handle handle_{};
    };
}