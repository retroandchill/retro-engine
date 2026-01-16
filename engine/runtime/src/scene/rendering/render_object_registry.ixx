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
    using RenderObjectFactory = std::function<RenderObject &(ViewportID)>;

    export class RETRO_API RenderObjectRegistry
    {
        RenderObjectRegistry() = default;
        ~RenderObjectRegistry() = default;

      public:
        static RenderObjectRegistry &instance();

        void register_type(Name type_name, RenderObjectFactory creator);

        [[nodiscard]] RenderObject &create(Name type_name, ViewportID viewport_id) const;

      private:
        std::unordered_map<Name, RenderObjectFactory> factories_{};
    };

    export struct RenderObjectTypeRegistration
    {
        RETRO_API RenderObjectTypeRegistration(Name type_name, RenderObjectFactory creator);
    };

} // namespace retro
