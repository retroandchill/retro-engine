# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RUNTIME_SOURCES src/engine.cpp
        src/scene/scene.cpp
        src/scene/scene_c.cpp
        src/engine_c.cpp
        src/scene/rendering/geometry.cpp
        src/scene/rendering/rendering.cpp
        src/assets/assets.cpp
)

set(RETRO_RUNTIME_MODULES
        src/runtime.ixx
        src/scene/scene.ixx
        src/scene/rendering/rendering.ixx
        src/scene/rendering/geometry.ixx
        src/assets/assets.ixx
        src/assets/texture.ixx
)

set(RETRO_RUNTIME_HEADERS
        include/retro/runtime/scene/scene.h
        include/retro/runtime/engine.h
        src/assets/texture.cpp
)


add_library(retro_runtime
        ${RETRO_RUNTIME_SOURCES}
        ${RETRO_RUNTIME_MODULES}
        ${RETRO_RUNTIME_HEADERS}
)

target_include_directories(retro_runtime PUBLIC include)

retro_add_boilerplate(retro_runtime)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_runtime PUBLIC RETRO_STATIC)
endif()

target_sources(retro_runtime PUBLIC
        FILE_SET CXX_MODULES FILES
            ${RETRO_RUNTIME_MODULES}
        )

target_link_libraries(retro_runtime PUBLIC retro_core retro_logging retro_platform)

find_package(Stb REQUIRED)
target_include_directories(retro_runtime PRIVATE ${Stb_INCLUDE_DIR})

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
