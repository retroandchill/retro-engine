#version 450

layout(location = 0) in vec2 vUV;
layout(location = 0) out vec4 outColor;

layout(push_constant) uniform QuadData {
    vec4 color;
    vec2 position;
    vec2 size;
    vec2 viewportSize;
} uQuad;

void main()
{
    outColor = uQuad.color;  // solid per-quad color
}