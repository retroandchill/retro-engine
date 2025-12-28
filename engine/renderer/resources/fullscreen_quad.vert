#version 450

layout(location = 0) out vec2 vUV;

void main()
{
    const vec2 positions[6] = vec2[](
    // first triangle
    vec2(-1.0, -1.0), // bottom-left
    vec2(-1.0,  1.0), // top-left
    vec2( 1.0, -1.0), // bottom-right

    // second triangle
    vec2(-1.0,  1.0), // top-left
    vec2( 1.0,  1.0), // top-right
    vec2( 1.0, -1.0)  // bottom-right
    );

    const vec2 uvs[6] = vec2[](
        // must match the same corners as positions[]
        vec2(0.0, 0.0), // bottom-left
        vec2(0.0, 1.0), // top-left
        vec2(1.0, 0.0), // bottom-right

        vec2(0.0, 1.0), // top-left
        vec2(1.0, 1.0), // top-right
        vec2(1.0, 0.0)  // bottom-right
    );

    gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
    vUV = uvs[gl_VertexIndex];
}