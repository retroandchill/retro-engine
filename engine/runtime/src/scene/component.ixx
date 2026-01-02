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
    export using ComponentID = DefaultHandle;

    export class RETRO_API Component
    {
      public:
        using IdType = ComponentID;

      protected:
        inline explicit Component(const ComponentID &id, class Entity &owner) : id_{id}, entity_{&owner}
        {
        }

      public:
        virtual ~Component() = default;

        [[nodiscard]] inline ComponentID id() const
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
        ComponentID id_;
        Entity *entity_;
    };
} // namespace retro