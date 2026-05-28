vcpkg_from_github(
        OUT_SOURCE_PATH SOURCE_PATH
        REPO talesm/SDL3pp
        REF "${VERSION}"
        SHA512 b45b7bd19be81ccdc371af22e4e8b63e8dd3f386330ee1841275b5b8ec025a18beb33e055450bdec6da3f3e797ee72c9a27fea0d6996b20aeccf6f8a196d974f
        HEAD_REF main
)

vcpkg_cmake_configure(
        SOURCE_PATH "${SOURCE_PATH}"
        OPTIONS
        -DBUILD_EXAMPLES=OFF
        -DSDL3PP_BUILD_EXAMPLES_AGAINST_AMALGAMATION=OFF
)

vcpkg_cmake_install()

file(INSTALL "${SOURCE_PATH}/include/SDL3pp" DESTINATION "${CURRENT_PACKAGES_DIR}/include/")
file(INSTALL "${CMAKE_CURRENT_LIST_DIR}/usage" DESTINATION "${CURRENT_PACKAGES_DIR}/share/${PORT}")
vcpkg_install_copyright(FILE_LIST "${SOURCE_PATH}/LICENSE.txt")
