#version 450
#include "common/camera.glsl"
#include "common/depth.glsl"

// Vertex attributes from the buffer
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inUV;

layout(location = 2) in mat2 inTransform;
layout(location = 4) in vec2 inTranslation;
layout(location = 5) in int inZOrder;
layout(location = 6) in vec2 inPivot;
layout(location = 7) in vec2 inSize;
layout(location = 8) in vec4 inColor;
layout(location = 9) in uint inHasTexture;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vColor;
layout(location = 2) out flat uint vHasTexture;


layout(push_constant) uniform SceneDataBlock {
    CameraData uData;
};

void main() {
    vec2 localPos = (inPosition - inPivot) * inSize;
    vec2 worldPos = inTransform * localPos + inTranslation;

    vec2 cameraFinalPos = translate_to_camera_space(uData, worldPos);

    vec2 ndc = cameraFinalPos * 2.0 - 1.0;
    float depth = calculateDepth(inZOrder);
    gl_Position = vec4(ndc, depth, 1.0);
    vUV = inUV;
    vColor = inColor;
    vHasTexture = inHasTexture;
}
