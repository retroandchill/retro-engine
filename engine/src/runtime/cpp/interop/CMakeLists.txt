# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_INTEROP_SOURCES
        private/logger.cpp
        private/engine.cpp
        private/scene.cpp
        private/assets.cpp
        private/name.cpp
        private/localization/locale.cpp
        private/localization/decimal_format.cpp
        private/localization/date_format.cpp
        private/localization/calendar.cpp
        private/localization/time_zone.cpp
        private/localization/collator.cpp
        private/localization/plural_rules.cpp
        private/platform.cpp
        private/rendering.cpp
        private/async.cpp
)

set(RETRO_INTEROP_PRIVATE_MODULES
        private/localization/icu.ixx
        private/localization/buffers.ixx
)

set(RETRO_INTEROP_PUBLIC_MODULES
        public/interop_error.ixx
)

add_library(retro_interop SHARED
        ${RETRO_INTEROP_SOURCES}
        ${RETRO_INTEROP_PRIVATE_MODULES}
        ${RETRO_INTEROP_PUBLIC_MODULES}
)

retro_add_boilerplate(retro_interop)

target_sources(retro_interop PUBLIC
        FILE_SET retro_interop_public_modules
        TYPE CXX_MODULES FILES
        ${RETRO_INTEROP_PUBLIC_MODULES})

target_sources(retro_interop PRIVATE
        FILE_SET retro_interop_private_modules
        TYPE CXX_MODULES FILES
        ${RETRO_INTEROP_PRIVATE_MODULES})

find_package(ICU COMPONENTS uc i18n REQUIRED)
target_link_libraries(retro_interop PRIVATE
            retro_core
            retro_logging
            retro_platform
            retro_runtime
            retro_renderer
            ICU::uc
            ICU::i18n)

set_target_properties(retro_interop PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
