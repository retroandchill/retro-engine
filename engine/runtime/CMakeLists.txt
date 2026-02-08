# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RUNTIME_SOURCES src/engine.cpp
        src/world/scene.cpp
        src/world/scene_c.cpp
        src/engine_c.cpp
        src/rendering/objects/geometry.cpp
        src/rendering/pipeline_manager.cpp
        src/assets/asset_manager.cpp
        src/assets/assets_c.cpp
        src/rendering/objects/sprite.cpp
        src/assets/asset.cpp
        src/assets/filesystem_asset_source.cpp
        src/assets/textures/texture_decoder.cpp
        src/rendering/texture_render_data.cpp
        src/world/scene_node.cpp
        src/world/viewport.cpp
)

set(RETRO_RUNTIME_HEADERS
        include/retro/runtime/scene/scene.h
        include/retro/runtime/engine.h
        src/assets/textures/texture.cpp
        include/retro/runtime/assets/assets.h
)

set(RETRO_RUNTIME_MODULES
        src/world/scene.ixx
        src/rendering/renderer2d.ixx
        src/rendering/objects/geometry.ixx
        src/assets/asset_manager.ixx
        src/assets/textures/texture.ixx
        src/engine.ixx
        src/rendering/objects/sprite.ixx
        src/script_runtime.ixx
        src/assets/asset_path.ixx
        src/assets/asset.ixx
        src/assets/filesystem_asset_source.ixx
        src/rendering/texture_render_data.ixx
        src/assets/asset_source.ixx
        src/assets/asset_decoder.ixx
        src/assets/asset_load_result.ixx
        src/assets/textures/texture_decoder.ixx
        src/rendering/shader_layout.ixx
        src/rendering/draw_command.ixx
        src/rendering/render_pipeline.ixx
        src/rendering/pipeline_manager.ixx
        src/world/scene_node.ixx
        src/world/viewport.ixx
        src/rendering/layout/margin.ixx
        src/rendering/layout/anchors.ixx
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
