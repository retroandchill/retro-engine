/**
 * @file utils.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import boost;

namespace retro::filesystem
{
    std::filesystem::path get_executable_path()
    {
        return boost::dll::program_location().parent_path();
    }
} // namespace retro::filesystem
