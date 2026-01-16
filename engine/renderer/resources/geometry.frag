#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;

layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform sampler2D texSampler;

layout(push_constant) uniform SceneData {
    vec2 viewportSize;
    vec2 position;
    uint zOrder;
    float rotation;
    vec2 scale;
    uint hasTexture;
} uData;

void main() {
    vec4 texColor = vec4(1.0);

    if (uData.hasTexture != 0) {
        texColor = texture(texSampler, vUV);
    }

    outColor = texColor * vColor;
}
