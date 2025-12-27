add_library(retro_platform)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_platform PUBLIC RETRO_STATIC)
endif()

target_sources(retro_platform PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/window.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_platform PUBLIC
        retro_core
        retro_runtime
        SDL3::SDL3)

set_target_properties(retro_platform PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")