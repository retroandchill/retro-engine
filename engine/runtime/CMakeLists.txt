add_library(retro_runtime
        src/engine.cpp
        src/loading/shared_library.cpp
        src/filesystem/utils.cpp
)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_runtime PUBLIC RETRO_STATIC)
endif()

target_sources(retro_runtime PUBLIC
        FILE_SET CXX_MODULES FILES
        src/engine.ixx
        src/index.ixx
        src/interfaces/renderer_2d.ixx
        src/interfaces/index.ixx
        src/interfaces/script_runtime.ixx
        src/sdl_runtime.ixx
        src/loading/index.ixx
        src/loading/shared_library.ixx
        src/filesystem/utils.ixx
        src/filesystem/index.ixx)

target_link_libraries(retro_runtime PUBLIC retro_core retro_sdl)

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")