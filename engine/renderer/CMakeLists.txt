set(RENDERER_SHADER_DIR "${CMAKE_CURRENT_SOURCE_DIR}/resources")
set(RENDERER_SHADER_OUTPUT_DIR "${CMAKE_CURRENT_BINARY_DIR}/shaders")

set(RENDERER_SHADERS
        "${RENDERER_SHADER_DIR}/fullscreen_quad.vert"
        "${RENDERER_SHADER_DIR}/solid_color.frag"
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

add_library(retro_renderer
        src/vulkan_renderer2d.cpp
        src/components/vulkan_device.cpp
        src/components/vulkan_swapchain.cpp
        src/components/vulkan_command_pool.cpp
        src/components/vulkan_sync.cpp
        src/window.cpp
        src/pipeline/pipeline_manager.cpp
        src/instances/quad.cpp
        src/pipeline/pipeline_registry.cpp
)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_renderer PUBLIC RETRO_STATIC)
endif()

target_sources(retro_renderer PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/vulkan_renderer2d.ixx
        src/vulkan_viewport.ixx
        src/window.ixx
        src/components/index.ixx
        src/components/vulkan_device.ixx
        src/components/vulkan_swapchain.ixx
        src/components/vulkan_command_pool.ixx
        src/components/vulkan_sync.ixx
        src/pipeline/index.ixx
        src/pipeline/pipeline_manager.ixx
        src/pipeline/render_pipeline.ixx
        src/instances/index.ixx
        src/instances/quad.ixx
        src/pipeline/pipeline_registry.ixx
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