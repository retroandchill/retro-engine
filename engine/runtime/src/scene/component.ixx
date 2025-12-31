//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.component;

namespace retro
{
    export class RETRO_API Component
    {
    public:
        virtual ~Component() = default;

        virtual void on_attach() = 0;
        virtual void on_detach() = 0;
    };
}