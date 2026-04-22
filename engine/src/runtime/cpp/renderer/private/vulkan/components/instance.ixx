/**
 * @file instance.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

export module retro.renderer.vulkan.components.instance;

import vulkan;
import retro.platform.window;
import retro.renderer.vulkan.components.surface;

namespace retro
{
    export class VulkanInstance
    {
      public:
        explicit VulkanInstance(WindowBackend backend);

        static VulkanInstance create(WindowBackend backend);

        [[nodiscard]] inline vk::UniqueSurfaceKHR create_surface(const Window &window) const
        {
            return retro::create_surface(window, instance_.get());
        }

        [[nodiscard]] inline std::vector<vk::PhysicalDevice> enumerate_physical_devices() const
        {
            return instance_->enumeratePhysicalDevices();
        }

      private:
        vk::UniqueInstance instance_;
        std::unique_ptr<vk::detail::DispatchLoaderDynamic> dldi_{};
        vk::UniqueHandle<vk::DebugUtilsMessengerEXT, vk::detail::DispatchLoaderDynamic> debug_messenger_{};
    };
} // namespace retro
