add_library(retro_core SHARED)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        src/core.ixx
        src/strings/concepts.ixx
        src/strings/strings.ixx
        src/strings/cstring_view.ixx)