/**
 * @file instance.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.scopes.instance;

import vulkan_hpp;
import retro.platform.window;

namespace retro
{
    export class VulkanInstance
    {
        explicit inline VulkanInstance(vk::UniqueInstance instance) : instance_{std::move(instance)}
        {
        }

      public:
        static VulkanInstance create(WindowBackend backend);

      private:
        vk::UniqueInstance instance_;
    };
} // namespace retro
