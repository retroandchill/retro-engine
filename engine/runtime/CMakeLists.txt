# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_RUNTIME_SOURCES src/engine.cpp
        src/filesystem/utils.cpp
        src/scene/scene.cpp
        src/scene/rendering/render_object.cpp
        src/scene/actor_ptr.cpp
        src/scene/scene_c.cpp
        src/scene/rendering/render_object_registry.cpp
        src/engine_c.cpp
        src/scene/rendering/geometry_render_object.cpp
        src/scene/rendering/pipeline_registry.cpp
        src/scene/rendering/registration.cpp)

set(RETRO_RUNTIME_MODULES src/engine.ixx
        src/index.ixx
        src/interfaces/renderer_2d.ixx
        src/interfaces/index.ixx
        src/interfaces/script_runtime.ixx
        src/sdl_runtime.ixx
        src/filesystem/utils.ixx
        src/filesystem/index.ixx
        src/scene/index.ixx
        src/scene/scene.ixx
        src/scene/viewport.ixx
        src/scene/rendering/render_object.ixx
        src/scene/rendering/index.ixx
        src/scene/rendering/render_proxy_manager.ixx
        src/scene/rendering/render_proxy_bucket.ixx
        src/scene/transform.ixx
        src/scene/actor_ptr.ixx
        src/scene/rendering/render_object_registry.ixx
        src/scene/rendering/geometry_render_object.ixx
        src/scene/rendering/render_pipeline.ixx
        src/scene/rendering/pipeline_registry.ixx
        src/assets/index.ixx
        src/assets/textures/index.ixx
        src/assets/textures/texture_loader.ixx
        src/assets/textures/texture.ixx)

set(RETRO_RUNTIME_HEADERS
        include/retro/runtime/scene/scene.h
        include/retro/runtime/engine.h)


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

target_link_libraries(retro_runtime PUBLIC retro_core retro_logging retro_sdl retro_boost)

set_target_properties(retro_runtime PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
