/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

#include <SDL3/SDL_vulkan.h>

module retro.renderer.vulkan.services;

import vulkan_hpp;
import retro.logging;
import retro.core.type_traits.callable;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.renderer.vulkan.renderer;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.swapchain;

namespace retro
{
    namespace
    {
        std::span<const char *const> get_required_instance_extensions(const Window &viewport)
        {
            switch (auto [backend, handle] = viewport.native_handle(); backend)
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

        vk::UniqueInstance create_instance(const Window &viewport)
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

            const auto extensions = get_required_instance_extensions(viewport);

            std::vector validation_feature_enables = {vk::ValidationFeatureEnableEXT::eDebugPrintf};

            vk::ValidationFeaturesEXT validation_features{
                .enabledValidationFeatureCount = static_cast<std::uint32_t>(validation_feature_enables.size()),
                .pEnabledValidationFeatures = validation_feature_enables.data()};

            const vk::InstanceCreateInfo create_info{
                .pNext = &validation_features,
                .pApplicationInfo = &app_info,
                .enabledLayerCount = static_cast<std::uint32_t>(enabled_layers.size()),
                .ppEnabledLayerNames = enabled_layers.data(),
                .enabledExtensionCount = static_cast<std::uint32_t>(extensions.size()),
                .ppEnabledExtensionNames = extensions.data()};

            return vk::createInstanceUnique(create_info);
        }

        vk::UniqueSurfaceKHR create_surface(const Window &viewport, vk::Instance instance)
        {
            switch (auto [backend, handle] = viewport.native_handle(); backend)
            {
                case WindowBackend::SDL3:
                    {
                        vk::SurfaceKHR::CType surface;
                        if (!SDL_Vulkan_CreateSurface(static_cast<SDL_Window *>(handle), instance, nullptr, &surface))
                        {
                            throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
                        }

                        return vk::UniqueSurfaceKHR{surface, instance};
                    }
            }

            throw std::runtime_error{"Unsupported window backend"};
        }
    } // namespace

    void add_vulkan_services(ServiceCollection &services)
    {
        services.add_singleton<Renderer2D, VulkanRenderer2D>()
            .add_singleton<&create_instance>()
            .add_singleton<&create_surface>()
            .add_singleton<VulkanDevice>()
            .add_singleton<VulkanBufferManager>();
    }
} // namespace retro
