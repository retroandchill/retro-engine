//
// Created by fcors on 12/25/2025.
//
module;

#include "retro/core/exports.h"

export module retro.scripting:interop;

export import :interop.name_exporter;

namespace retro {
    export RETRO_API void register_all_exports();
}