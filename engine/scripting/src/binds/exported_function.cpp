//
// Created by fcors on 12/25/2025.
//
module retro.scripting.binds;

namespace retro::scripting {
    ExportedFunction::ExportedFunction(const Name module_name, const Name function_name, void *function_ptr, const usize function_size) : name(function_name), function_ptr(function_ptr), function_size(function_size) {
        BindsManager::register_exported_function(module_name, *this);
    }
}
