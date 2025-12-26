//
// Created by fcors on 12/25/2025.
//
module;

#include "retro/core/exports.h"

export module retro.scripting.interop;

export import :name_exporter;

namespace retro::scripting {
    export RETRO_API void register_all_exports();
}