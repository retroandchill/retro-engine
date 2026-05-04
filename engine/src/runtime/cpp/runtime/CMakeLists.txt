# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RENDERER_SHADER_DIR "${CMAKE_CURRENT_SOURCE_DIR}/resources")
set(RENDERER_SHADER_OUTPUT_DIR "${CMAKE_CURRENT_BINARY_DIR}/shaders")

set(RENDERER_SHADERS
        "${RENDERER_SHADER_DIR}/geometry.vert"
        "${RENDERER_SHADER_DIR}/geometry.frag"
        "${RENDERER_SHADER_DIR}/sprite.vert"
        "${RENDERER_SHADER_DIR}/sprite.frag"
)

set(COMPILED_SHADERS "")

foreach(SHADER ${RENDERER_SHADERS})
        get_filename_component(SHADER_NAME "${SHADER}" NAME)
        set(SPV "${RENDERER_SHADER_OUTPUT_DIR}/${SHADER_NAME}.spv")

        add_custom_command(
                OUTPUT "${SPV}"
                COMMAND "${CMAKE_COMMAND}" -E make_directory "${RENDERER_SHADER_OUTPUT_DIR}"
                COMMAND "${GLSLC_EXECUTABLE}" -g -I"." --target-env=vulkan1.2 "${SHADER}" -o "${SPV}"
                DEPENDS "${SHADER}"
                COMMENT "Compiling GLSL shader ${SHADER_NAME} -> ${SPV}"
                VERBATIM
        )

        list(APPEND COMPILED_SHADERS "${SPV}")
endforeach()

set(EMBED_SHADERS_TEMPLATE ${CMAKE_SOURCE_DIR}/templates/embedded_shader.ixx.mustache)
set(EMBED_SHADERS_SCRIPT ${CMAKE_SOURCE_DIR}/engine/src/programs/python/shader-embedder/embed_shaders.py)
set(OUTPUT_SHADER_FILE "${CMAKE_CURRENT_BINARY_DIR}/shaders.ixx")
find_package(Python3 REQUIRED COMPONENTS Interpreter)
add_custom_command(
        OUTPUT "${OUTPUT_SHADER_FILE}"
        COMMAND "${Python3_EXECUTABLE}" "${EMBED_SHADERS_SCRIPT}" --input-files ${COMPILED_SHADERS} --output-file "${OUTPUT_SHADER_FILE}" --template-file "${EMBED_SHADERS_TEMPLATE}"
        DEPENDS
                ${COMPILED_SHADERS}
                "${EMBED_SHADERS_SCRIPT}"
                "${EMBED_SHADERS_TEMPLATE}"
        COMMENT "Compiling GLSL shader ${SHADER_NAME} -> ${SPV}"
        VERBATIM
)

add_custom_target(renderer_shaders ALL
        DEPENDS "${OUTPUT_SHADER_FILE}"
)

set(RETRO_RUNTIME_SOURCES private/engine.cpp
        private/world/scene.cpp
        private/rendering/objects/geometry.cpp
        private/rendering/pipeline_manager.cpp
        private/rendering/objects/sprite.cpp
        private/world/scene_node.cpp
        private/world/viewport.cpp
        private/rendering/render_manager.cpp
        private/rendering/headless_render_backend.cpp
)

set(RETRO_RUNTIME_HEADERS
)

set(RETRO_RUNTIME_PUBLIC_MODULES
        public/modules/world/scene.ixx
        public/modules/rendering/renderer2d.ixx
        public/modules/rendering/objects/geometry.ixx
        public/modules/engine.ixx
        public/modules/rendering/objects/sprite.ixx
        public/modules/rendering/shader_layout.ixx
        public/modules/rendering/draw_command.ixx
        public/modules/rendering/render_pipeline.ixx
        public/modules/rendering/pipeline_manager.ixx
        public/modules/world/scene_node.ixx
        public/modules/world/viewport.ixx
        public/modules/rendering/layout/margin.ixx
        public/modules/rendering/layout/anchors.ixx
        public/modules/exceptions.ixx
        public/modules/rendering/render_manager.ixx
        public/modules/rendering/render_backend.ixx
        public/modules/rendering/headless_render_backend.ixx
        public/modules/rendering/headless_renderer2d.ixx
        public/modules/rendering/texture.ixx
)

set(RETRO_RUNTIME_PRIVATE_MODULES
        "${OUTPUT_SHADER_FILE}")


add_library(retro_runtime ${INTERMEDIATE_LIB_TYPE}
        ${RETRO_RUNTIME_SOURCES}
        ${RETRO_RUNTIME_HEADERS}
        ${RETRO_RUNTIME_PUBLIC_MODULES}
)

target_include_directories(retro_runtime PUBLIC public/include)

retro_add_boilerplate(retro_runtime)

target_sources(retro_runtime PUBLIC
        FILE_SET retro_runtime_public
        TYPE CXX_MODULES FILES
            ${RETRO_RUNTIME_PUBLIC_MODULES}
        )

target_sources(retro_runtime PRIVATE
        FILE_SET retro_runtime_private
        TYPE CXX_MODULES FILES
        BASE_DIRS ${CMAKE_CURRENT_SOURCE_DIR} ${CMAKE_CURRENT_BINARY_DIR}
        FILES ${RETRO_RUNTIME_PRIVATE_MODULES}
)

target_link_libraries(retro_runtime PUBLIC retro_core retro_logging retro_platform)

add_dependencies(retro_runtime renderer_shaders)

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}")
