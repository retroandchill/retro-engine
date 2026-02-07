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
#include <vulkan/vk_platform.h>

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

        void add_if_missing(std::vector<const char *> &exts, const char *name)
        {
            const bool exists =
                std::ranges::any_of(exts, [&](const char *e) { return std::string_view{e} == std::string_view{name}; });
            if (!exists)
            {
                exts.push_back(name);
            }
        }

        VKAPI_ATTR vk::Bool32 VKAPI_CALL debug_callback(vk::DebugUtilsMessageSeverityFlagBitsEXT message_severity,
                                                        vk::DebugUtilsMessageTypeFlagsEXT,
                                                        const vk::DebugUtilsMessengerCallbackDataEXT *callback_data,
                                                        void *)
        {
            auto logger = get_logger();

            LogLevel level = LogLevel::Info;
            if (message_severity & vk::DebugUtilsMessageSeverityFlagBitsEXT::eError)
            {
                level = LogLevel::Error;
            }
            else if (message_severity & vk::DebugUtilsMessageSeverityFlagBitsEXT::eWarning)
            {
                level = LogLevel::Warn;
            }
            else if (message_severity & vk::DebugUtilsMessageSeverityFlagBitsEXT::eInfo)
            {
                level = LogLevel::Info;
            }
            else
            {
                level = LogLevel::Debug;
            }

            const char *msg = (callback_data && callback_data->pMessage) ? callback_data->pMessage : "";

            logger.log(level, msg);

            // VK_FALSE = don't abort the call.
            return vk::False;
        }
    } // namespace

    VulkanInstance::VulkanInstance(
        vk::UniqueInstance instance,
        std::unique_ptr<vk::detail::DispatchLoaderDynamic> dldi,
        vk::UniqueHandle<vk::DebugUtilsMessengerEXT, vk::detail::DispatchLoaderDynamic> debug_messenger)
        : instance_{std::move(instance)}, dldi_{std::move(dldi)}, debug_messenger_{std::move(debug_messenger)}
    {
    }

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

        auto extensions = get_required_instance_extensions(backend) | std::ranges::to<std::vector>();
#ifndef NDEBUG
        add_if_missing(extensions, vk::EXTDebugUtilsExtensionName);
#endif

        std::vector validation_feature_enables = {vk::ValidationFeatureEnableEXT::eDebugPrintf};

        vk::ValidationFeaturesEXT validation_features{.enabledValidationFeatureCount =
                                                          static_cast<std::uint32_t>(validation_feature_enables.size()),
                                                      .pEnabledValidationFeatures = validation_feature_enables.data()};

        vk::InstanceCreateInfo create_info{.pNext = &validation_features,
                                           .pApplicationInfo = &app_info,
                                           .enabledLayerCount = static_cast<std::uint32_t>(enabled_layers.size()),
                                           .ppEnabledLayerNames = enabled_layers.data(),
                                           .enabledExtensionCount = static_cast<std::uint32_t>(extensions.size()),
                                           .ppEnabledExtensionNames = extensions.data()};

#ifndef NDEBUG
        vk::DebugUtilsMessengerCreateInfoEXT messenger_ci{
            .pNext = create_info.pNext,
            .messageSeverity =
                vk::DebugUtilsMessageSeverityFlagBitsEXT::eVerbose | vk::DebugUtilsMessageSeverityFlagBitsEXT::eInfo |
                vk::DebugUtilsMessageSeverityFlagBitsEXT::eWarning | vk::DebugUtilsMessageSeverityFlagBitsEXT::eError,
            .messageType = vk::DebugUtilsMessageTypeFlagBitsEXT::eGeneral |
                           vk::DebugUtilsMessageTypeFlagBitsEXT::eValidation |
                           vk::DebugUtilsMessageTypeFlagBitsEXT::ePerformance,
            .pfnUserCallback = debug_callback,
            .pUserData = nullptr};
        create_info.pNext = &messenger_ci;
#endif

        auto instance = vk::createInstanceUnique(create_info);

        auto dldi = std::make_unique<vk::detail::DispatchLoaderDynamic>(
            instance.get(),
            [](const vk::Instance::CType native_instance, const char *name)
            { return vk::Instance{native_instance}.getProcAddr(name); });

#ifndef NDEBUG
        auto messenger = instance->createDebugUtilsMessengerEXTUnique(messenger_ci, nullptr, *dldi);
#else
        vk::UniqueHandle<vk::DebugUtilsMessengerEXT, vk::detail::DispatchLoaderDynamic> messenger{};
#endif

        return VulkanInstance{std::move(instance), std::move(dldi), std::move(messenger)};
    }
} // namespace retro
