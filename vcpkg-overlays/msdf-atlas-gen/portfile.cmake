vcpkg_from_github(
        OUT_SOURCE_PATH SOURCE_PATH
        REPO Chlumsky/msdf-atlas-gen
        REF "v${VERSION}"
        SHA512 97e921f9f3f64d9a7e7c4b525f9368213cebd1f7e00f1d8c5b5075acb93be854fff498e5b627f10d0a35c36b0e1b26073bdfdd56a239234f5043ca49fc497181
        HEAD_REF master
)

vcpkg_check_features(OUT_FEATURE_OPTIONS FEATURE_OPTIONS
        FEATURES
        "geometry-preprocessing"     MSDF_ATLAS_USE_SKIA
)

vcpkg_cmake_configure(
        SOURCE_PATH "${SOURCE_PATH}"
        OPTIONS
        -DMSDF_ATLAS_MSDFGEN_EXTERNAL=ON
        -DMSDF_ATLAS_USE_VCPKG=OFF
        -DMSDF_ATLAS_INSTALL=ON
        -DMSDF_ATLAS_NO_ARTERY_FONT=ON
        -DMSDF_ATLAS_DYNAMIC_RUNTIME=ON
        ${FEATURE_OPTIONS}
)

vcpkg_cmake_install()

vcpkg_cmake_config_fixup(CONFIG_PATH lib/cmake/msdf-atlas-gen
        TOOLS_PATH bin)

vcpkg_copy_pdbs()

file(REMOVE_RECURSE "${CURRENT_PACKAGES_DIR}/debug/include")

vcpkg_install_copyright(FILE_LIST "${SOURCE_PATH}/LICENSE.txt")
file(INSTALL "${CMAKE_CURRENT_LIST_DIR}/usage" DESTINATION "${CURRENT_PACKAGES_DIR}/share/${PORT}")
