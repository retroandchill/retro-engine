#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;
layout(location = 2) in float vPixelRange;

layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform sampler2D fontAtlas;

float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange()
{
    vec2 unitRange = vec2(vPixelRange) / vec2(textureSize(fontAtlas, 0));
    vec2 screenTexSize = vec2(1.0) / fwidth(vUV);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

void main() {
    vec3 msd = texture(fontAtlas, vUV).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDist = screenPxRange() * (sd - 0.5);
    float alpha = clamp(screenPxDist + 0.5, 0.0, 1.0);

    outColor = vec4(vColor.rgb, vColor.a * alpha);

    if (outColor.a < 0.001) {
        discard;
    }
}
