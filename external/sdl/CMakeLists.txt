add_library(retro_sdl STATIC)

target_sources(retro_sdl PUBLIC
        FILE_SET CXX_MODULES FILES
        sdl.ixx)


find_package(SDL3 REQUIRED)
target_link_libraries(retro_sdl
        PUBLIC
            SDL3::SDL3
        PRIVATE
            retro_vulkan)

set_target_properties(retro_sdl PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")