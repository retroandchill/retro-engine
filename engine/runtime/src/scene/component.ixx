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
    export class RETRO_API Component : public std::enable_shared_from_this<Component>
    {
      public:
        virtual ~Component() = default;

        [[nodiscard]] inline uint64 id() const
        {
            return id_;
        }

        virtual void on_attach() = 0;
        virtual void on_detach() = 0;

        template <typename Self>
        std::shared_ptr<std::remove_cvref_t<Self>> shared_from_this(this Self &self)
        {
            return std::static_pointer_cast<std::remove_cvref_t<Self>>(
                self.std::template enable_shared_from_this<Component>::shared_from_this());
        }

        template <typename Self>
        std::weak_ptr<std::remove_cvref_t<Self>> weak_this(this Self &self)
        {
            return std::static_pointer_cast<std::remove_reference_t<Self>>(
                self.std::template enable_shared_from_this<Component>::shared_from_this());
        }

      private:
        uint64 id_;
    };
} // namespace retro