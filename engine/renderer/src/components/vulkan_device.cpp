//
// Created by fcors on 12/26/2025.
//
module retro.renderer;

import vulkan_hpp;

namespace retro
{
    VulkanDevice::VulkanDevice(vk::Instance instance, vk::SurfaceKHR surface)
        : physical_device_{pick_physical_device(instance, surface, graphics_family_index_, present_family_index_)},
          device_{create_device(physical_device_, graphics_family_index_, present_family_index_)},
          graphics_queue_{device_->getQueue(graphics_family_index_, 0)},
          present_queue_{device_->getQueue(present_family_index_, 0)}
    {
    }

    vk::PhysicalDevice VulkanDevice::pick_physical_device(vk::Instance instance,
                                                          vk::SurfaceKHR surface,
                                                          uint32 &out_graphics_family,
                                                          uint32 &out_present_family)
    {
        auto devices = instance.enumeratePhysicalDevices();

        for (auto dev : devices)
        {
            uint32 graphics_family = std::numeric_limits<uint32>::max();
            uint32 present_family = std::numeric_limits<uint32>::max();

            if (is_device_suitable(dev, surface, graphics_family, present_family))
            {
                out_graphics_family = graphics_family;
                out_present_family = present_family;
                return dev;
            }
        }

        throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
    }

    bool VulkanDevice::is_device_suitable(const vk::PhysicalDevice device,
                                          const vk::SurfaceKHR surface,
                                          uint32 &out_graphics_family,
                                          uint32 &out_present_family)
    {
        auto families = device.getQueueFamilyProperties();

        out_graphics_family = std::numeric_limits<uint32>::max();
        out_present_family = std::numeric_limits<uint32>::max();

        for (uint32 i = 0; i < families.size(); ++i)
        {
            if (families[i].queueFlags & vk::QueueFlagBits::eGraphics)
            {
                out_graphics_family = i;
            }

            vk::Bool32 present_support = vk::False;
            auto res = device.getSurfaceSupportKHR(i, surface, &present_support);
            if (res == vk::Result::eSuccess && present_support == vk::True)
            {
                out_present_family = i;
            }

            if (out_graphics_family != std::numeric_limits<uint32>::max() &&
                out_present_family != std::numeric_limits<uint32>::max())
            {
                break;
            }
        }

        if (out_graphics_family == std::numeric_limits<uint32>::max() ||
            out_present_family == std::numeric_limits<uint32>::max())
        {
            return false;
        }

        // Check swapchain support (at least one format & present mode)
        uint32 format_count = 0;
        auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
        if (res != vk::Result::eSuccess || format_count == 0)
        {
            return false;
        }

        uint32 present_mode_count = 0;
        res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
        if (res != vk::Result::eSuccess || present_mode_count == 0)
        {
            return false;
        }

        return true;
    }

    vk::UniqueDevice VulkanDevice::create_device(const vk::PhysicalDevice physical_device,
                                                 const uint32 graphics_family,
                                                 const uint32 present_family)
    {
        // Required device extensions
        constexpr std::array DEVICE_EXTENSIONS = {vk::KHRSwapchainExtensionName};

        std::set unique_families{graphics_family, present_family};

        float queue_priority = 1.0f;
        std::vector<vk::DeviceQueueCreateInfo> queue_infos;
        queue_infos.reserve(unique_families.size());

        for (uint32 family : unique_families)
        {
            queue_infos.emplace_back(vk::DeviceQueueCreateFlags{}, family, 1, &queue_priority);
        }

        vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

        vk::DeviceCreateInfo create_info{{},
                                         static_cast<uint32>(queue_infos.size()),
                                         queue_infos.data(),
                                         0,
                                         nullptr,
                                         DEVICE_EXTENSIONS.size(),
                                         DEVICE_EXTENSIONS.data(),
                                         &device_features};

        return physical_device.createDeviceUnique(create_info, nullptr);
    }
} // namespace retro