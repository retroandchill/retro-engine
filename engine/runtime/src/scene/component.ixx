/**
 * @file component.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.component;

import std;
import retro.core;
import :scene.actor_ptr;

namespace retro
{
    export class RETRO_API Component
    {
      public:
        using IdType = ComponentID;

      protected:
        inline explicit Component(const ComponentID id, const EntityID entity_id) : id_{id}, entity_{entity_id}
        {
        }

      public:
        virtual ~Component() = default;

        [[nodiscard]] inline ComponentID id() const
        {
            return id_;
        }

        [[nodiscard]] class Entity &entity() const noexcept;

        virtual void on_attach() = 0;
        virtual void on_detach() = 0;

      private:
        ComponentID id_;
        ActorPtr<Entity> entity_;
    };

} // namespace retro
