//
// Created by fcors on 12/29/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.scene2d;

import std;
import retro.core;
import :scene.entity;

namespace retro
{
    export class RETRO_API Scene2D
    {
    public:
        
    private:
        std::vector<Entity> entities;
    };
}