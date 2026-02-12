#ifndef COMMON_CAMERA
#define COMMON_CAMERA
struct CameraData {
    vec2 viewportSize;
    vec2 cameraPosition;
    vec2 cameraPivot;
    vec2 cameraRotation;
    float cameraZoom;
};

vec2 translate_to_camera_space(in CameraData sceneData, in vec2 worldPos) {
    vec2 normalizedCameraPos = sceneData.cameraPosition / sceneData.viewportSize;

    vec2 cameraTranslatedPos = worldPos / sceneData.viewportSize - normalizedCameraPos;

    mat2 cameraRotMat = mat2(
    sceneData.cameraRotation.x, sceneData.cameraRotation.y,
    -sceneData.cameraRotation.y, sceneData.cameraRotation.x
    );

    vec2 cameraRotatedPos = cameraRotMat * cameraTranslatedPos + sceneData.cameraPivot;

    return cameraRotatedPos * sceneData.cameraZoom;
}
#endif
