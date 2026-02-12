#ifndef COMMON_DEPTH
#define COMMON_DEPTH
const float DEPTH_RANGE = 1000000.0f;

float calculateDepth(in int zOrder) {
    float depth = float(-zOrder) / 1000000.0f + 0.5f;
    return clamp(depth, 0.0f, 1.0f);
}
#endif
