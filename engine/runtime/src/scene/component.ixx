//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.component;

import std;
import retro.core;

namespace retro
{
    export class RETRO_API Component
    {
      protected:
        static uint64 next_id_;

        inline explicit Component(class Entity &owner) : id_{next_id_++}, entity_{&owner}
        {
        }

      public:
        virtual ~Component() = default;

        [[nodiscard]] inline uint64 id() const
        {
            return id_;
        }

        inline Entity &entity() const noexcept
        {
            return *entity_;
        }

        virtual void on_attach() = 0;
        virtual void on_detach() = 0;

      private:
        uint64 id_;
        Entity *entity_;
    };

    uint64 Component::next_id_ = 0;
} // namespace retro