#version 450
#extension GL_EXT_debug_printf : enable

// Vertex attributes from the buffer
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec4 inColor;

struct InstanceData {
    vec2 translation;
    mat2 transform;
    vec2 pivot;
    vec2 size;
    uint has_texture;
    uint padding[3];
};

layout(std430, set = 0, binding = 0) readonly buffer InstanceBuffer {
    InstanceData instances[];
} instanceData;

layout(location = 0) out vec2 vUV;
layout(location = 1) out vec4 vColor;
layout(location = 2) out flat uint vHasTexture;


layout(push_constant) uniform SceneData {
    vec2 viewportSize;
} uData;

void main() {
    InstanceData instance = instanceData.instances[gl_InstanceIndex];

    vec2 localPos = inPosition * instance.size - instance.pivot;
    vec2 transformedPos = instance.transform * localPos + instance.translation;

    vec2 ndc = (transformedPos / uData.viewportSize) * 2.0 - 1.0;
    gl_Position = vec4(ndc, 0.0, 1.0);
    vUV = inUV;
    vColor = inColor;
    vHasTexture = instance.has_texture;
}
