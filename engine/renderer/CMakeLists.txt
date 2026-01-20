# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RENDERER_SHADER_DIR "${CMAKE_CURRENT_SOURCE_DIR}/resources")
set(RENDERER_SHADER_OUTPUT_DIR "${CMAKE_CURRENT_BINARY_DIR}/shaders")

set(RENDERER_SHADERS
        "${RENDERER_SHADER_DIR}/geometry.vert"
        "${RENDERER_SHADER_DIR}/geometry.frag"
)

set(COMPILED_SHADERS "")

foreach(SHADER ${RENDERER_SHADERS})
    get_filename_component(SHADER_NAME "${SHADER}" NAME)
    set(SPV "${RENDERER_SHADER_OUTPUT_DIR}/${SHADER_NAME}.spv")

    add_custom_command(
            OUTPUT "${SPV}"
            COMMAND "${CMAKE_COMMAND}" -E make_directory "${RENDERER_SHADER_OUTPUT_DIR}"
            COMMAND "${GLSLC_EXECUTABLE}" -g --target-env=vulkan1.2 "${SHADER}" -o "${SPV}"
            DEPENDS "${SHADER}"
            COMMENT "Compiling GLSL shader ${SHADER_NAME} -> ${SPV}"
            VERBATIM
    )

    list(APPEND COMPILED_SHADERS "${SPV}")
endforeach()

add_custom_target(renderer_shaders ALL
        DEPENDS ${COMPILED_SHADERS}
)

set(RUNTIME_SHADER_DIR "${RETRO_BIN_DIR}/shaders")
add_custom_command(TARGET renderer_shaders POST_BUILD
        COMMAND "${CMAKE_COMMAND}" -E make_directory "${RUNTIME_SHADER_DIR}"
        COMMAND "${CMAKE_COMMAND}" -E copy_directory "${RENDERER_SHADER_OUTPUT_DIR}" "${RUNTIME_SHADER_DIR}"
        COMMENT "Copying compiled shaders to runtime directory"
)

set(RETRO_RENDER_SOURCES src/vulkan_renderer2d.cpp
        src/components/vulkan_device.cpp
        src/components/vulkan_swapchain.cpp
        src/components/vulkan_command_pool.cpp
        src/components/vulkan_sync.cpp
        src/window.cpp
        src/pipeline/vulkan_pipeline_manager.cpp
        src/pipeline/vulkan_render_pipeline.cpp
        src/vulkan_buffer_manager.cpp
)

set(RETRO_RENDER_MODULES src/index.ixx
        src/vulkan_renderer2d.ixx
        src/vulkan_viewport.ixx
        src/window.ixx
        src/components/index.ixx
        src/components/vulkan_device.ixx
        src/components/vulkan_swapchain.ixx
        src/components/vulkan_command_pool.ixx
        src/components/vulkan_sync.ixx
        src/pipeline/index.ixx
        src/pipeline/vulkan_pipeline_manager.ixx
        src/pipeline/vulkan_render_pipeline.ixx
        src/vulkan_buffer_manager.ixx
)

set(RETRO_RENDER_HEADERS )

add_library(retro_renderer
        ${RETRO_RENDER_SOURCES}
        ${RETRO_RENDER_MODULES}
        ${RETRO_RENDER_HEADERS}
)

target_include_directories(retro_renderer PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)

retro_add_boilerplate(retro_renderer)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_renderer PUBLIC RETRO_STATIC)
endif()

target_sources(retro_renderer PUBLIC
        FILE_SET CXX_MODULES FILES
        ${RETRO_RENDER_MODULES}
        )

target_link_libraries(retro_renderer
        PUBLIC
            retro_runtime
            retro_vulkan)

add_dependencies(retro_renderer renderer_shaders)

set_target_properties(retro_renderer PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
