//
// Created by fcors on 12/25/2025.
//
module;

#include "retro/core/exports.h"

export module retro.scripting:binds_manager;

import std;
import retro.core;
import :exported_function;

namespace retro {
    export class BindsManager {
        BindsManager() = default;
        ~BindsManager() = default;

        static BindsManager& instance();

    public:
        BindsManager(const BindsManager&) = delete;
        BindsManager(BindsManager&&) = delete;
        BindsManager& operator=(const BindsManager&) = delete;
        BindsManager& operator=(BindsManager&&) = delete;

        RETRO_API static void register_exported_function(Name module_name, const ExportedFunction& exported_function);
        RETRO_API static void* get_bound_function(const char16_t* module_name, int32 module_length, const char16_t* function_name, int32 function_length, int32 param_size);

    private:
        std::unordered_map<Name, std::vector<ExportedFunction>> exported_functions_;
    };
}