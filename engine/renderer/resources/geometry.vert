#version 450
#extension GL_EXT_debug_printf : enable

// Vertex attributes from the buffer
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec4 inColor;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vColor;

// Push constants matching GeometryRenderData + Viewport info
layout(push_constant) uniform SceneData {
    vec2 viewportSize; // You'll likely want to pass this here or via a UBO
    mat3 worldMatrix;
    uint hasTexture;
} uData;

void main() {
    vec3 transformedPos = uData.worldMatrix * vec3(inPosition, 1.0);

    // Map pixels to NDC: [0, Width] -> [-1, 1], [0, Height] -> [-1, 1]
    // Note: Vulkan Y is down, so we might need to flip or handle it via the projection
    vec2 ndc = (transformedPos.xy / uData.viewportSize) * 2.0 - 1.0;

    gl_Position = vec4(ndc, 0.0, 1.0);
    vUV = inUV;
    vColor = inColor;
}
