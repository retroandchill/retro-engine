# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_runtime
        src/engine.cpp
        src/filesystem/utils.cpp
        src/scene/rendering/render_component.cpp
        src/scene/scene2d.cpp
        src/scene/component.cpp
        src/scene/actor_ptr.cpp
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
        src/filesystem/utils.ixx
        src/filesystem/index.ixx
        src/scene/index.ixx
        src/scene/scene2d.ixx
        src/scene/entity.ixx
        src/scene/component.ixx
        src/scene/rendering/index.ixx
        src/scene/rendering/render_component.ixx
        src/scene/rendering/render_proxy_manager.ixx
        src/scene/rendering/render_proxy_bucket.ixx
        src/scene/transform.ixx
        src/scene/actor_ptr.ixx)

target_link_libraries(retro_runtime PUBLIC retro_core retro_logging retro_sdl retro_boost)

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
