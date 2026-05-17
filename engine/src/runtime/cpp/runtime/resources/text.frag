#version 450

layout(location = 0) in vec2 vUV;
layout(location = 1) in vec4 vColor;

layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform sampler2D fontAtlas;

void main() {
    float distance = texture(fontAtlas, vUV).r;
    float smoothing = fwidth(distance);
    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);

    outColor = vec4(vColor.rgb, vColor.a * alpha);

    if (outColor.a < 0.001) {
        discard;
    }
}
