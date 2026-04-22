/**
 * @file device.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.components.device;

import retro.core.containers.optional;
import retro.platform.window;
import retro.renderer.vulkan.components.surface;
import retro.core.io.file_stream;

namespace retro
{
    namespace
    {
        Optional<std::pair<std::uint32_t, std::uint32_t>> find_graphics_and_present_families(
            const vk::PhysicalDevice device,
            const vk::SurfaceKHR surface)
        {
            auto families = device.getQueueFamilyProperties();

            std::uint32_t out_graphics_family = vk::QueueFamilyIgnored;
            std::uint32_t out_present_family = vk::QueueFamilyIgnored;

            for (std::uint32_t i = 0; i < families.size(); ++i)
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

                if (out_graphics_family != vk::QueueFamilyIgnored && out_present_family != vk::QueueFamilyIgnored)
                {
                    break;
                }
            }

            if (out_graphics_family == std::numeric_limits<std::uint32_t>::max() ||
                out_present_family == std::numeric_limits<std::uint32_t>::max())
            {
                return std::nullopt;
            }

            // Check swapchain support (at least one format & present mode)
            std::uint32_t format_count = 0;
            auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
            if (res != vk::Result::eSuccess || format_count == 0)
            {
                return std::nullopt;
            }

            std::uint32_t present_mode_count = 0;
            res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
            if (res != vk::Result::eSuccess || present_mode_count == 0)
            {
                return std::nullopt;
            }

            return std::make_pair(out_graphics_family, out_present_family);
        }

        VulkanDeviceConfig pick_physical_device(const VulkanInstance &instance, const vk::SurfaceKHR surface)
        {
            for (const auto devices = instance.enumerate_physical_devices(); const auto dev : devices)
            {
                auto result = find_graphics_and_present_families(dev, surface);
                if (!result.has_value())
                {
                    continue;
                }

                auto [graphics_family, present_family] = *result;
                return VulkanDeviceConfig{dev, graphics_family, present_family};
            }

            throw GraphicsException{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_device(const VulkanDeviceConfig &config)
        {
            // Required device extensions
            constexpr std::array device_extensions = {vk::KHRSwapchainExtensionName};

            const std::set unique_families{config.graphics_family, config.present_family};

            constexpr float queue_priority = 1.0f;
            std::vector<vk::DeviceQueueCreateInfo> queue_infos;
            queue_infos.reserve(unique_families.size());

            for (const std::uint32_t family : unique_families)
            {
                queue_infos.emplace_back(vk::DeviceQueueCreateInfo{.queueFamilyIndex = family,
                                                                   .queueCount = 1,
                                                                   .pQueuePriorities = &queue_priority});
            }

            vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

            const vk::DeviceCreateInfo create_info{.queueCreateInfoCount =
                                                       static_cast<std::uint32_t>(queue_infos.size()),
                                                   .pQueueCreateInfos = queue_infos.data(),
                                                   .enabledExtensionCount = device_extensions.size(),
                                                   .ppEnabledExtensionNames = device_extensions.data(),
                                                   .pEnabledFeatures = &device_features};

            return config.physical_device.createDeviceUnique(create_info, nullptr);
        }
    } // namespace

    void VulkanStagingBuffer::copy_to_buffer(const std::span<const std::byte> buffer)
    {
        auto *data = device_.mapMemory(memory_.get(), 0, size_);
        std::memcpy(data, buffer.data(), buffer.size());
        device_.unmapMemory(memory_.get());
    }

    VulkanDevice::VulkanDevice(const VulkanInstance &instance, PlatformBackend &platform_backend)
    {
        // Create a hidden window and surface to test against for the device so we can properly separate device
        // selection from surface creation.
        const auto window =
            platform_backend.create_window(WindowDesc{.flags = WindowFlags::vulkan | WindowFlags::hidden});
        if (!window.has_value())
        {
            throw GraphicsException{"VulkanDevice: failed to create hidden window"};
        }
        auto surface = instance.create_surface(**window);
        const auto config = pick_physical_device(instance, surface.get());

        physical_device_ = config.physical_device;
        device_ = create_device(config);
        graphics_family_index_ = config.graphics_family;
        present_family_index_ = config.present_family;
        graphics_queue_ = device_->getQueue(graphics_family_index_, 0);
        present_queue_ = device_->getQueue(present_family_index_, 0);
    }

    std::unique_ptr<VulkanDevice> VulkanDevice::create(const VulkanInstance &instance,
                                                       PlatformBackend &platform_backend)
    {
        return std::make_unique<VulkanDevice>(instance, platform_backend);
    }

    vk::UniquePipeline VulkanDevice::create_graphics_pipeline(const vk::PipelineCache cache,
                                                              const vk::GraphicsPipelineCreateInfo &create_info) const
    {
        auto [result, pipeline] = device_->createGraphicsPipelineUnique(cache, create_info);
        if (result != vk::Result::eSuccess)
        {
            throw GraphicsException{"VulkanRenderer2D: failed to create graphics pipeline"};
        }

        return std::move(pipeline);
    }

    StreamResult<vk::UniqueShaderModule> VulkanDevice::create_shader_module(const std::filesystem::path &path) const
    {
        return FileStream::open(path, FileOpenMode::read_only)
            .and_then([this](const std::unique_ptr<FileStream> &stream) { return create_shader_module(*stream); });
    }

    StreamResult<vk::UniqueShaderModule> VulkanDevice::create_shader_module(Stream &stream) const
    {
        return stream.read_all().and_then(
            [this](std::span<const std::byte> bytes) -> StreamResult<vk::UniqueShaderModule>
            {
                const auto *code = reinterpret_cast<const std::uint32_t *>(bytes.data());

                if (bytes.size() % sizeof(std::uint32_t) != 0)
                {
                    return std::unexpected{StreamError::invalid_argument};
                }

                const vk::ShaderModuleCreateInfo info{.codeSize = bytes.size(), .pCode = code};
                return device_->createShaderModuleUnique(info);
            });
    }

    VulkanBufferManager VulkanDevice::create_buffer_manager(const std::size_t pool_size) const
    {
        const vk::BufferCreateInfo buffer_info{
            .size = pool_size,
            .usage = vk::BufferUsageFlagBits::eVertexBuffer | vk::BufferUsageFlagBits::eIndexBuffer |
                     vk::BufferUsageFlagBits::eStorageBuffer,
        };
        auto buffer = device_->createBufferUnique(buffer_info);

        const auto mem_reqs = device_->getBufferMemoryRequirements(buffer.get());

        const vk::MemoryAllocateInfo alloc_info{
            .allocationSize = mem_reqs.size,
            .memoryTypeIndex =
                find_memory_type(mem_reqs.memoryTypeBits,
                                 vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent)};

        auto memory = device_->allocateMemoryUnique(alloc_info);
        device_->bindBufferMemory(buffer.get(), memory.get(), 0);
        const auto mapped_ptr = device_->mapMemory(memory.get(), 0, pool_size);
        return VulkanBufferManager(std::move(buffer), std::move(memory), mapped_ptr, pool_size);
    }

    VulkanStagingBuffer VulkanDevice::create_staging_buffer(vk::DeviceSize size) const
    {
        const vk::BufferCreateInfo staging_info{.size = size,
                                                .usage = vk::BufferUsageFlagBits::eTransferSrc,
                                                .sharingMode = vk::SharingMode::eExclusive};

        auto staging_buffer = device_->createBufferUnique(staging_info);

        const auto mem_req = device_->getBufferMemoryRequirements(staging_buffer.get());

        const vk::MemoryAllocateInfo alloc_info{
            .allocationSize = mem_req.size,
            .memoryTypeIndex =
                find_memory_type(mem_req.memoryTypeBits,
                                 vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent)};

        auto staging_memory = device_->allocateMemoryUnique(alloc_info);
        device_->bindBufferMemory(staging_buffer.get(), staging_memory.get(), 0);
        return VulkanStagingBuffer{device_.get(), std::move(staging_buffer), std::move(staging_memory), mem_req.size};
    }

    std::uint32_t VulkanDevice::find_memory_type(const std::uint32_t type_filter,
                                                 vk::MemoryPropertyFlags properties) const
    {
        const auto mem_properties = physical_device_.getMemoryProperties();

        for (std::uint32_t i = 0; i < mem_properties.memoryTypeCount; ++i)
        {
            if (type_filter & (1 << i) && (mem_properties.memoryTypes[i].propertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw GraphicsException("VulkanBufferManager: failed to find suitable memory type!");
    }

    vk::UniqueCommandPool VulkanDevice::create_command_pool() const
    {
        const vk::CommandPoolCreateInfo pool_info{.flags = vk::CommandPoolCreateFlagBits::eResetCommandBuffer,
                                                  .queueFamilyIndex = graphics_family_index_};

        return device_->createCommandPoolUnique(pool_info);
    }

    vk::UniqueSampler VulkanDevice::create_linear_sampler() const
    {
        constexpr vk::SamplerCreateInfo sampler_info{
            .magFilter = vk::Filter::eNearest,
            .minFilter = vk::Filter::eNearest,
            .mipmapMode = vk::SamplerMipmapMode::eNearest,
            .addressModeU = vk::SamplerAddressMode::eClampToEdge,
            .addressModeV = vk::SamplerAddressMode::eClampToEdge,
            .addressModeW = vk::SamplerAddressMode::eClampToEdge,
            .mipLodBias = 0.0f,
            .anisotropyEnable = vk::False,
            .maxAnisotropy = 1.0f,
            .compareEnable = vk::False,
            .compareOp = vk::CompareOp::eAlways,
            .minLod = 0.0f,
            .maxLod = 0.0f,
            .borderColor = vk::BorderColor::eIntOpaqueBlack,
            .unnormalizedCoordinates = vk::False,
        };

        return device_->createSamplerUnique(sampler_info);
    }

    vk::UniqueDeviceMemory VulkanDevice::allocate_image_memory(const vk::Image image) const
    {
        const auto img_mem_req = device_->getImageMemoryRequirements(image);

        const vk::MemoryAllocateInfo img_alloc_info{
            .allocationSize = img_mem_req.size,
            .memoryTypeIndex = find_memory_type(img_mem_req.memoryTypeBits, vk::MemoryPropertyFlagBits::eDeviceLocal)};

        auto img_memory = device_->allocateMemoryUnique(img_alloc_info);
        device_->bindImageMemory(image, img_memory.get(), 0);
        return img_memory;
    }
} // namespace retro
