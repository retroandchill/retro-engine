# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RENDERER_SOURCES private/vulkan/renderer.cpp
        private/vulkan/components/pipeline.cpp
        private/vulkan/components/device.cpp
        private/vulkan/components/buffer_manager.cpp
        private/services.cpp
        private/vulkan/components/instance.cpp
        private/vulkan/components/surface.cpp
        private/vulkan/components/presenter.cpp
        private/vulkan/vulkan_render_backend.cpp
)

set(RETRO_RENDERER_HEADERS )

set(RETRO_RENDERER_PUBLIC_MODULES
        public/modules/services.ixx
)

set(RETRO_RENDERER_PRIVATE_MODULES
        private/vulkan/renderer.ixx
        private/vulkan/components/pipeline.ixx
        private/vulkan/components/device.ixx
        private/vulkan/components/buffer_manager.ixx
        private/vulkan/components/instance.ixx
        private/vulkan/components/surface.ixx
        private/vulkan/components/presenter.ixx
        private/vulkan/vulkan_render_backend.ixx
)

add_library(retro_renderer ${INTERMEDIATE_LIB_TYPE}
        ${RETRO_RENDERER_SOURCES}
        ${RETRO_RENDERER_HEADERS}
        ${RETRO_RENDERER_PUBLIC_MODULES}
        ${RETRO_RENDERER_PRIVATE_MODULES}
)

target_include_directories(retro_renderer PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/public/include)

retro_add_boilerplate(retro_renderer)

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

set_target_properties(retro_renderer PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
