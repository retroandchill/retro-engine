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
            COMMAND "${GLSLC_EXECUTABLE}" -g -I"${CMAKE_CURRENT_SOURCE_DIR}" --target-env=vulkan1.2 "${SHADER}" -o "${SPV}"
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

set(RETRO_RENDERER_SOURCES private/src/vulkan/renderer.cpp
        private/src/vulkan/components/sync.cpp
        private/src/vulkan/components/pipeline.cpp
        private/src/vulkan/components/device.cpp
        private/src/vulkan/components/swapchain.cpp
        private/src/vulkan/components/buffer_manager.cpp
        private/src/services.cpp
        private/src/vulkan/services.cpp
        private/src/vulkan/components/instance.cpp
        private/src/vulkan/components/viewport_renderer.cpp
        private/src/vulkan/components/surface.cpp
        private/src/vulkan/texture_manager.cpp
)

set(RETRO_RENDERER_HEADERS )

set(RETRO_RENDERER_PUBLIC_MODULES
        public/src/services.ixx
)

set(RETRO_RENDERER_PRIVATE_MODULES
        private/src/vulkan/renderer.ixx
        private/src/vulkan/components/sync.ixx
        private/src/vulkan/components/pipeline.ixx
        private/src/vulkan/components/device.ixx
        private/src/vulkan/components/swapchain.ixx
        private/src/vulkan/components/buffer_manager.ixx
        private/src/vulkan/services.ixx
        private/src/vulkan/components/instance.ixx
        private/src/vulkan/components/viewport_renderer.ixx
        private/src/vulkan/components/surface.ixx
        private/src/vulkan/texture_manager.ixx
)

add_library(retro_renderer
        ${RETRO_RENDERER_SOURCES}
        ${RETRO_RENDERER_HEADERS}
        ${RETRO_RENDERER_PUBLIC_MODULES}
        ${RETRO_RENDERER_PRIVATE_MODULES}
)

target_include_directories(retro_renderer PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/public/include)

retro_add_boilerplate(retro_renderer)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_renderer PUBLIC RETRO_STATIC)
endif()

target_sources(retro_renderer PUBLIC
        FILE_SET retro_renderer_public
        TYPE CXX_MODULES FILES
        ${RETRO_RENDERER_PUBLIC_MODULES}
        )

target_sources(retro_renderer PRIVATE
        FILE_SET retro_renderer_private
        TYPE CXX_MODULES FILES
        ${RETRO_RENDERER_PRIVATE_MODULES}
)

target_link_libraries(retro_renderer
        PUBLIC
            retro_runtime
            Vulkan::Cppm)

add_dependencies(retro_renderer renderer_shaders)

set_target_properties(retro_renderer PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
