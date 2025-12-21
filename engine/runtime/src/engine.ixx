//
// Created by fcors on 12/21/2025.
//
module;

#include <chrono>
#include <retro/core/exports.h>

export module retro.runtime.engine;

import std;
import retro.core;
import retro.core.strings;
import retro.platform;
import retro.platform.window;

namespace retro::runtime {
    using namespace core;
    using namespace platform;

    export class RETRO_API Engine {

    public:
        Engine(const CStringView name, const int32 width, const int32 height) : window_{platform_, width, height, name} {}

        void run() {
            using clock = std::chrono::steady_clock;
            constexpr float target_frame_time = 1.0f / 60.0f; // 60 FPS

            auto last_time = clock::now();

            while (true) {
                auto now = clock::now();
                std::chrono::duration<float> frame_delta = now - last_time;
                last_time = now;

                const float delta_time = frame_delta.count();

                if (platform_.poll_events()) {
                    break;
                }

                tick(delta_time);
                render();

                if (delta_time < target_frame_time) {
                    auto sleep_time = std::chrono::duration<float>(target_frame_time - delta_time);

                    std::this_thread::sleep_for(
                    std::chrono::duration_cast<std::chrono::milliseconds>(sleep_time)
                    );
                }
            }
        }

    private:
        void tick(float delta_time) {
            // TODO: Add tick logic
        }

        void render() {
            // TODO: Add render logic
        }

        Platform platform_;
        Window window_;
    };
}