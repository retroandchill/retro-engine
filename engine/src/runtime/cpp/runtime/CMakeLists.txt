# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RUNTIME_SOURCES private/engine.cpp
        private/world/scene.cpp
        private/rendering/objects/geometry.cpp
        private/rendering/pipeline_manager.cpp
        private/assets/asset_manager.cpp
        private/rendering/objects/sprite.cpp
        private/assets/asset.cpp
        private/assets/filesystem_asset_source.cpp
        private/assets/textures/texture_decoder.cpp
        private/rendering/texture_manager.cpp
        private/world/scene_node.cpp
        private/world/viewport.cpp
        private/rendering/renderer2d.cpp
)

set(RETRO_RUNTIME_HEADERS
        private/assets/textures/texture.cpp
)

set(RETRO_RUNTIME_MODULES
        public/modules/world/scene.ixx
        public/modules/rendering/renderer2d.ixx
        public/modules/rendering/objects/geometry.ixx
        public/modules/assets/asset_manager.ixx
        public/modules/assets/textures/texture.ixx
        public/modules/engine.ixx
        public/modules/rendering/objects/sprite.ixx
        public/modules/assets/asset_path.ixx
        public/modules/assets/asset.ixx
        public/modules/assets/filesystem_asset_source.ixx
        public/modules/rendering/texture_manager.ixx
        public/modules/assets/asset_source.ixx
        public/modules/assets/asset_decoder.ixx
        public/modules/assets/asset_load_result.ixx
        public/modules/assets/textures/texture_decoder.ixx
        public/modules/rendering/shader_layout.ixx
        public/modules/rendering/draw_command.ixx
        public/modules/rendering/render_pipeline.ixx
        public/modules/rendering/pipeline_manager.ixx
        public/modules/world/scene_node.ixx
        public/modules/world/viewport.ixx
        public/modules/rendering/layout/margin.ixx
        public/modules/rendering/layout/anchors.ixx
)


add_library(retro_runtime ${INTERMEDIATE_LIB_TYPE}
        ${RETRO_RUNTIME_SOURCES}
        ${RETRO_RUNTIME_MODULES}
        ${RETRO_RUNTIME_HEADERS}
)

target_include_directories(retro_runtime PUBLIC public/include)

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
