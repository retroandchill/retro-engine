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
        explicit VulkanInstance(
            vk::UniqueInstance instance,
            std::unique_ptr<vk::detail::DispatchLoaderDynamic> dldi,
            vk::UniqueHandle<vk::DebugUtilsMessengerEXT, vk::detail::DispatchLoaderDynamic> debug_messenger);

      public:
        static VulkanInstance create(WindowBackend backend);

        [[nodiscard]] vk::Instance handle() const noexcept
        {
            return instance_.get();
        }

      private:
        vk::UniqueInstance instance_;
        std::unique_ptr<vk::detail::DispatchLoaderDynamic> dldi_{};
        vk::UniqueHandle<vk::DebugUtilsMessengerEXT, vk::detail::DispatchLoaderDynamic> debug_messenger_{};
    };
} // namespace retro
