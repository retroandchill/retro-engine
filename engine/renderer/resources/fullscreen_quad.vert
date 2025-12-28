#version 450

layout(push_constant) uniform QuadData {
    vec4 color;         // RGBA
    vec2 position;      // in pixels, top-left
    vec2 size;          // in pixels
    vec2 viewportSize;  // window width/height in pixels
} uQuad;

layout(location = 0) out vec2 vUV;

void main()
{
    // 6 vertices, defined in local [0,1] quad space
    const vec2 localPos[6] = vec2[](
        vec2(0.0, 0.0), // bottom-left
        vec2(0.0, 1.0), // top-left
        vec2(1.0, 0.0), // bottom-right

        vec2(0.0, 1.0),  // top-left
        vec2(1.0, 1.0),   // top-right
        vec2(1.0, 0.0)  // bottom-right
    );

    // Convert local [0,1] to pixel coordinates (origin: top-left)
    vec2 pixelPos = uQuad.position + localPos[gl_VertexIndex] * uQuad.size;

    // Map pixels → NDC
    vec2 ndc;
    ndc.x = (pixelPos.x / uQuad.viewportSize.x) * 2.0 - 1.0;
    ndc.y = (pixelPos.y / uQuad.viewportSize.y) * 2.0 - 1.0;

    gl_Position = vec4(ndc, 0.0, 1.0);

    // Top-down UVs: (0,0) = top-left, (1,1) = bottom-right
    vUV = localPos[gl_VertexIndex];
}