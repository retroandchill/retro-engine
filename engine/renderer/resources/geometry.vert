#version 450

// Vertex attributes from the buffer
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inUV;

layout(location = 2) in mat2 inTransform;
layout(location = 4) in vec2 inTranslation;
layout(location = 5) in vec2 inPivot;
layout(location = 6) in vec2 inSize;
layout(location = 7) in vec4 inColor;
layout(location = 8) in uint inHasTexture;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vColor;
layout(location = 2) out flat uint vHasTexture;


layout(push_constant) uniform SceneData {
    vec2 viewportSize;
} uData;

void main() {
    vec2 localPos = (inPosition - inPivot) * inSize;
    vec2 transformedPos = inTransform * localPos + inTranslation;

    vec2 ndc = (transformedPos / uData.viewportSize) * 2.0 - 1.0;
    gl_Position = vec4(ndc, 0.0, 1.0);
    vUV = inUV;
    vColor = inColor;
    vHasTexture = inHasTexture;
}
