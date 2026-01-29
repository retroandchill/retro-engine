#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;
layout(location = 2) in flat uint vHasTexture;

layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform sampler2D texSampler;

void main() {
    vec4 texColor = vec4(1.0);

    if (vHasTexture != 0) {
        texColor = texture(texSampler, vUV);
    }

    outColor = texColor * vColor;
}
