/**
 * @file registration.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import retro.runtime;
import std;

namespace retro
{
    const RenderObjectTypeRegistration geometry_type_registration{
        "geometry",
        [](const ViewportID viewport_id) -> auto &
        {
            return Engine::instance().scene().create_render_object<GeometryRenderObject>(viewport_id);
        }};
    const PipelineRegistration geometry_pipeline_registration{"pipeline",
                                                              []
                                                              {
                                                                  return std::make_unique<GeometryRenderPipeline>();
                                                              }};
} // namespace retro
