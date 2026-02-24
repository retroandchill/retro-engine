#version 450
#include "common/camera.glsl"
#include "common/depth.glsl"

layout(location = 0) in mat2 inTransform;
layout(location = 2) in vec2 inTranslation;
layout(location = 3) in int inZOrder;
layout(location = 4) in vec2 inPivot;
layout(location = 5) in vec2 inSize;
layout(location = 6) in vec2 inMinUV;
layout(location = 7) in vec2 inMaxUV;
layout(location = 8) in vec4 inTint;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vTint;

layout(push_constant) uniform SceneDataBlock {
    CameraData uData;
};

const vec2 vertices[] = vec2[](
    vec2(0.0f, 0.0f),
    vec2(1.0f, 1.0f),
    vec2(1.0f, 0.0f),

    vec2(1.0f, 1.0f),
    vec2(0.0f, 0.0f),
    vec2(0.0f, 1.0f)
);

void main() {
    vec2 position = vertices[gl_VertexIndex];
    vec2 localPos = (position - inPivot) * inSize;
    vec2 spriteWorldPos = inTransform * localPos + inTranslation;

    vec2 cameraFinalPos = translate_to_camera_space(uData, spriteWorldPos);

    vec2 ndc = cameraFinalPos * 2.0 - 1.0;
    float depth = calculateDepth(inZOrder);
    gl_Position = vec4(ndc, depth, 1.0);

    vUV = inMinUV + ((inMaxUV - inMinUV) * position);
    vTint = inTint;
}
