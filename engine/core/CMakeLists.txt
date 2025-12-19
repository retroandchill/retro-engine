add_library(retro_core SHARED)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        src/core.ixx)