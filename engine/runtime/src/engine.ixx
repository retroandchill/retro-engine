//
// Created by fcors on 12/21/2025.
//
module;

#include <retro/core/exports.h>

export module retro.runtime.engine;

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
            while (!window_.should_close()) {
                window_.poll_events();
            }
        }

    private:
        Platform platform_;
        Window window_;
    };
}