/**
 * @file instance.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

#include <SDL3/SDL_vulkan.h>

module retro.renderer.vulkan.scopes.instance;

import retro.logging;

namespace retro
{
    namespace
    {
        std::span<const char *const> get_required_instance_extensions(const WindowBackend backend)
        {
            switch (backend)
            {
                case WindowBackend::SDL3:
                    {
                        std::uint32_t count = 0;
                        auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
                        if (names == nullptr)
                        {
                            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
                        }

                        return std::span{names, count};
                    }
            }

            get_logger().error("Unsupported window backend:");
            return {};
        }
    } // namespace

    VulkanInstance VulkanInstance::create(const WindowBackend backend)
    {
        vk::ApplicationInfo app_info{.pApplicationName = "Retro Engine",
                                     .applicationVersion = vk::makeVersion(1, 0, 0),
                                     .pEngineName = "Retro Engine",
                                     .engineVersion = vk::makeVersion(1, 0, 0),
                                     .apiVersion = vk::makeApiVersion(0, 1, 2, 0)};

        std::vector<const char *> enabled_layers;

#ifndef NDEBUG
        auto available_layers = vk::enumerateInstanceLayerProperties();
        const bool has_validation =
            std::ranges::any_of(available_layers,
                                [](const vk::LayerProperties &lp)
                                { return std::string_view{lp.layerName} == "VK_LAYER_KHRONOS_validation"; });

        if (has_validation)
        {
            enabled_layers.push_back("VK_LAYER_KHRONOS_validation");
        }
        else
        {
            get_logger().warn("Vulkan validation layers requested, but not available!");
        }
#endif

        const auto extensions = get_required_instance_extensions(backend);

        std::vector validation_feature_enables = {vk::ValidationFeatureEnableEXT::eDebugPrintf};

        vk::ValidationFeaturesEXT validation_features{.enabledValidationFeatureCount =
                                                          static_cast<std::uint32_t>(validation_feature_enables.size()),
                                                      .pEnabledValidationFeatures = validation_feature_enables.data()};

        const vk::InstanceCreateInfo create_info{.pNext = &validation_features,
                                                 .pApplicationInfo = &app_info,
                                                 .enabledLayerCount = static_cast<std::uint32_t>(enabled_layers.size()),
                                                 .ppEnabledLayerNames = enabled_layers.data(),
                                                 .enabledExtensionCount = static_cast<std::uint32_t>(extensions.size()),
                                                 .ppEnabledExtensionNames = extensions.data()};

        auto instance = vk::createInstanceUnique(create_info);

        return VulkanInstance{std::move(instance)};
    }
} // namespace retro
