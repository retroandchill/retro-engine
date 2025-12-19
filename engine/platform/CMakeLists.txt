add_library(retro_platform SHARED
        src/exports.h)

target_sources(retro_platform PUBLIC
        FILE_SET CXX_MODULES FILES
        src/window.ixx
        src/platform.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_platform PUBLIC SDL3::SDL3)