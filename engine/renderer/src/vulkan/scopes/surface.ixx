/**
 * @file surface.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.scopes.surface;

import vulkan_hpp;
import retro.platform.window;

namespace retro
{
    export class VulkanSurface
    {
      public:
        VulkanSurface(Window &window, vk::Instance instance);

      private:
        Window &window_;
        vk::UniqueSurfaceKHR surface_;
    };
} // namespace retro
