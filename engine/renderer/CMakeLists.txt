add_library(retro_renderer
        src/vulkan_renderer2d.cpp
        src/components/vulkan_device.cpp
        src/components/vulkan_swapchain.cpp
        src/components/vulkan_render_pass.cpp
        src/components/vulkan_framebuffers.cpp
        src/components/vulkan_command_pool.cpp
        src/components/vulkan_sync.cpp
)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_renderer PUBLIC RETRO_STATIC)
endif()

target_sources(retro_renderer PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/vulkan_renderer2d.ixx
        src/window.ixx
        src/components/index.ixx
        src/components/vulkan_instance.ixx
        src/components/vulkan_surface.ixx
        src/components/vulkan_device.ixx
        src/components/vulkan_swapchain.ixx
        src/components/vulkan_render_pass.ixx
        src/components/vulkan_framebuffers.ixx
        src/components/vulkan_command_pool.ixx
        src/components/vulkan_sync.ixx
        )

find_package(Vulkan REQUIRED)
target_link_libraries(retro_renderer
        PUBLIC
            retro_runtime
        PRIVATE
            Vulkan::Vulkan)

set_target_properties(retro_renderer PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")