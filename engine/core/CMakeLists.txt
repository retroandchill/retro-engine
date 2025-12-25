add_library(retro_core SHARED
        src/strings/name.cpp)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        src/core.ixx
        src/strings/concepts.ixx
        src/strings/strings.ixx
        src/strings/cstring_view.ixx
        src/strings/literals.ixx
        src/strings/name.ixx
        src/strings/comparison.ixx)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")