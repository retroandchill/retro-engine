#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;

layout(location = 0) out vec4 outColor;

void main() {
    // Combine the vertex color. If you add textures later, you'd multiply here.
    outColor = vColor;
}
