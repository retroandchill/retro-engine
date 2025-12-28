#version 450

layout(location = 0) in vec2 vUV;
layout(location = 0) out vec4 outColor;

// Hardcoded color for now; later we can use push constants or a UBO.
void main()
{
    outColor = vec4(vUV, 0.0, 1.0); // gradient to show things are working
}