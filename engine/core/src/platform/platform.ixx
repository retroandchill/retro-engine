/**
 * @file platform.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#define BOOST_DLL_USE_STD_FS
#include <boost/dll.hpp>

export module retro.core:platform;

// TODO: Move this from the public API into a shared location
export namespace boost::dll
{
    using dll::import_symbol;
    using dll::library_info;
    using dll::program_location;
    using dll::shared_library;
    using dll::symbol_location;
    using dll::symbol_location_ptr;
    using dll::operator==;
    using dll::operator!=;
    using dll::swap;
} // namespace boost::dll

namespace retro
{
    export [[nodiscard]] RETRO_API std::filesystem::path get_executable_path();
}
