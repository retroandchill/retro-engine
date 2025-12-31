//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.entity;

import std;
import :scene.component;

namespace retro
{
    export class RETRO_API Entity
    {
    public:

    private:
        std::vector<std::shared_ptr<Component>> components;
    };
}