add_library(retro_runtime
        src/engine.cpp)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_runtime PUBLIC RETRO_STATIC)
endif()

target_sources(retro_runtime PUBLIC
        FILE_SET CXX_MODULES FILES
        src/engine.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_runtime PUBLIC
        retro_core
        retro_platform
        retro_scripting)

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")