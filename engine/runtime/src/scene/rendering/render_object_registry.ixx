/**
 * @file render_object_registry.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_object_registry;

import retro.core;
import :scene.rendering.render_object;

namespace retro
{

    export class RETRO_API RenderObjectRegistry
    {
        RenderObjectRegistry() = default;
        ~RenderObjectRegistry() = default;

      public:
        using FactoryFunction = std::function<RenderObject &(ViewportID, std::span<const std::byte>)>;

        static RenderObjectRegistry &instance();

        void register_type(Name type_name, FactoryFunction creator);

        RenderObject &create(Name type_name, ViewportID viewport_id, std::span<const std::byte> payload = {}) const;

      private:
        std::unordered_map<Name, FactoryFunction> factories_;
    };

} // namespace retro
