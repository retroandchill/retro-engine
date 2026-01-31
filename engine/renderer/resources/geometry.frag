#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;
layout(location = 2) in flat uint vHasTexture;

layout(location = 0) out vec4 outColor;

void main() {
    vec4 texColor = vec4(1.0);

    outColor = texColor * vColor;
}
