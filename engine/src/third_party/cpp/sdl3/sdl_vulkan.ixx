/**
 * @file sdl_vulkan.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3pp/SDL3pp_vulkan.h>

export module sdl.vulkan;

export namespace SDL
{
    using SDL::Vulkan_CreateSurface;
    using SDL::Vulkan_DestroySurface;
    using SDL::Vulkan_GetInstanceExtensions;
    using SDL::Vulkan_GetPresentationSupport;
    using SDL::Vulkan_GetVkGetInstanceProcAddr;
    using SDL::Vulkan_LoadLibrary;
    using SDL::Vulkan_UnloadLibrary;
} // namespace SDL
