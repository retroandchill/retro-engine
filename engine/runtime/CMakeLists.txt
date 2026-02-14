# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RUNTIME_SOURCES private/src/engine.cpp
        private/src/world/scene.cpp
        private/src/world/scene_c.cpp
        private/src/engine_c.cpp
        private/src/rendering/objects/geometry.cpp
        private/src/rendering/pipeline_manager.cpp
        private/src/assets/asset_manager.cpp
        private/src/assets/assets_c.cpp
        private/src/rendering/objects/sprite.cpp
        private/src/assets/asset.cpp
        private/src/assets/filesystem_asset_source.cpp
        private/src/assets/textures/texture_decoder.cpp
        private/src/rendering/texture_manager.cpp
        private/src/world/scene_node.cpp
        private/src/world/viewport.cpp
        private/src/rendering/renderer2d.cpp
)

set(RETRO_RUNTIME_HEADERS
        public/include/retro/runtime/scene/scene_c.h
        public/include/retro/runtime/engine_c.h
        private/src/assets/textures/texture.cpp
        public/include/retro/runtime/assets/assets_c.h
)

set(RETRO_RUNTIME_MODULES
        public/src/world/scene.ixx
        public/src/rendering/renderer2d.ixx
        public/src/rendering/objects/geometry.ixx
        public/src/assets/asset_manager.ixx
        public/src/assets/textures/texture.ixx
        public/src/engine.ixx
        public/src/rendering/objects/sprite.ixx
        public/src/script_runtime.ixx
        public/src/assets/asset_path.ixx
        public/src/assets/asset.ixx
        public/src/assets/filesystem_asset_source.ixx
        public/src/rendering/texture_manager.ixx
        public/src/assets/asset_source.ixx
        public/src/assets/asset_decoder.ixx
        public/src/assets/asset_load_result.ixx
        public/src/assets/textures/texture_decoder.ixx
        public/src/rendering/shader_layout.ixx
        public/src/rendering/draw_command.ixx
        public/src/rendering/render_pipeline.ixx
        public/src/rendering/pipeline_manager.ixx
        public/src/world/scene_node.ixx
        public/src/world/viewport.ixx
        public/src/rendering/layout/margin.ixx
        public/src/rendering/layout/anchors.ixx
)


add_library(retro_runtime
        ${RETRO_RUNTIME_SOURCES}
        ${RETRO_RUNTIME_MODULES}
        ${RETRO_RUNTIME_HEADERS}
)

target_include_directories(retro_runtime PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/public/include)

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
