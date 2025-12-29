//
// Created by fcors on 12/23/2025.
//
module retro.runtime;

import boost;

namespace retro::filesystem
{
    std::filesystem::path get_executable_path()
    {
        return boost::dll::program_location().parent_path();
    }
} // namespace retro::filesystem
