/**
 * @file exported_function.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

module retro.core;

namespace retro
{
    ExportedFunction::ExportedFunction(const Name namespace_name,
                                       const Name function_name,
                                       void *function_ptr,
                                       const usize function_size)
        : name(function_name), function_ptr(function_ptr), function_size(function_size)
    {
        BindsManager::register_exported_function(namespace_name, *this);
    }
} // namespace retro
