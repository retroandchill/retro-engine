find_package(Python3 REQUIRED COMPONENTS Interpreter)
set(EMBED_FILES_SCRIPT ${CMAKE_SOURCE_DIR}/engine/src/programs/python/file-embedder/embed_files.py)

function(retro_embed_files TARGET_NAME OUTPUT_FILE TEMPLATE_FILE)
    set(one_value_args COMMENT DTYPE)
    set(list_args SOURCE_FILES DEPENDS)

    cmake_parse_arguments(PARSE "" "${one_value_args}" "${list_args}" ${ARGN})

    if ("${PARSE_DTYPE}" STREQUAL "")
        set(PARSE_DTYPE "uint8")
    endif()

    add_custom_command(
            OUTPUT "${OUTPUT_FILE}"
            COMMAND "${Python3_EXECUTABLE}" "${EMBED_FILES_SCRIPT}" --input-files ${PARSE_SOURCE_FILES} --output-file "${OUTPUT_FILE}" --template-file "${TEMPLATE_FILE}" --dtype "${PARSE_DTYPE}"
            DEPENDS
                "${EMBED_FILES_SCRIPT}"
                ${PARSE_SOURCE_FILES}
                ${TEMPLATE_FILE}
                ${PARSE_DEPENDS}
            COMMENT "${PARSE_COMMENT}"
            VERBATIM
    )

    add_custom_target(${TARGET_NAME} ALL
            DEPENDS "${OUTPUT_FILE}"
    )
endfunction()
