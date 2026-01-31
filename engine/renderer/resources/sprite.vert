#version 450

layout(location = 0) in mat2 inTransform;
layout(location = 2) in vec2 inTranslation;
layout(location = 3) in vec2 inPivot;
layout(location = 4) in vec2 inSize;
layout(location = 5) in vec2 inMinUV;
layout(location = 6) in vec2 inMaxUV;
layout(location = 7) in vec4 inTint;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vTint;

layout(push_constant) uniform SceneData {
    vec2 viewportSize;
} uData;

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
    vec2 transformedPos = inTransform * localPos + inTranslation;

    vec2 ndc = (transformedPos / uData.viewportSize) * 2.0 - 1.0;
    gl_Position = vec4(ndc, 0.0, 1.0);

    vUV = inMinUV + ((inMaxUV - inMinUV) * position);
    vTint = inTint;
}
