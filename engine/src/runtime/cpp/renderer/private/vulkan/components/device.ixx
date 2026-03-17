/**
 * @file device.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

export module retro.renderer.vulkan.components.device;

import vulkan;
import retro.core.di;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.buffer_manager;
import retro.platform.backend;
import retro.core.type_traits.basic;
import retro.core.io.stream;
import retro.runtime.exceptions;

namespace retro
{
    export struct VulkanDeviceConfig
    {
        vk::PhysicalDevice physical_device = nullptr;
        std::uint32_t graphics_family = vk::QueueFamilyIgnored;
        std::uint32_t present_family = vk::QueueFamilyIgnored;
    };

    export class VulkanDevice;

    export class VulkanStagingBuffer
    {
        inline VulkanStagingBuffer(vk::Device device,
                                   vk::UniqueBuffer buffer,
                                   vk::UniqueDeviceMemory memory,
                                   const vk::DeviceSize size)
            : device_{device}, buffer_{std::move(buffer)}, memory_{std::move(memory)}, size_{size}
        {
        }

      public:
        [[nodiscard]] inline vk::Buffer get() const noexcept
        {
            return buffer_.get();
        }

        void copy_to_buffer(std::span<const std::byte> data);

      private:
        friend class VulkanDevice;

        vk::Device device_;
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        vk::DeviceSize size_;
    };

    class VulkanDevice
    {
        constexpr static std::size_t default_pool_size = 1024 * 1024 * 10;

      public:
        VulkanDevice(const VulkanDeviceConfig &config, vk::UniqueDevice device);

        static std::unique_ptr<VulkanDevice> create(const VulkanInstance &instance, PlatformBackend &platform_backend);

        inline vk::UniquePipelineCache create_pipeline_cache(const vk::PipelineCacheCreateInfo &create_info = {}) const
        {
            return device_->createPipelineCacheUnique(create_info);
        }

        inline vk::UniquePipelineLayout create_pipeline_layout(const vk::PipelineLayoutCreateInfo &create_info) const
        {
            return device_->createPipelineLayoutUnique(create_info);
        }

        vk::UniquePipeline create_graphics_pipeline(vk::PipelineCache cache,
                                                    const vk::GraphicsPipelineCreateInfo &create_info) const;

        StreamResult<vk::UniqueShaderModule> create_shader_module(const std::filesystem::path &path) const;

        StreamResult<vk::UniqueShaderModule> create_shader_module(Stream &stream) const;

        template <std::invocable<vk::Queue> Functor>
        decltype(auto) submit_to_graphics_queue(Functor &&functor) const
        {
            std::scoped_lock graphics_queue_lock(graphics_queue_mutex_);
            return std::invoke(std::forward<Functor>(functor), graphics_queue_);
        }

        [[nodiscard]] std::uint32_t graphics_family() const noexcept
        {
            return graphics_family_index_;
        }

        template <std::invocable<vk::Queue> Functor>
        decltype(auto) submit_to_present_queue(Functor &&functor) const
        {
            std::scoped_lock graphics_queue_lock(present_queue_mutex_);
            return std::invoke(std::forward<Functor>(functor), present_queue_);
        }

        [[nodiscard]] std::uint32_t present_family() const noexcept
        {
            return present_family_index_;
        }

        inline vk::SurfaceCapabilitiesKHR get_surface_capabilities(const vk::SurfaceKHR surface) const
        {
            return physical_device_.getSurfaceCapabilitiesKHR(surface);
        }

        template <SimpleAllocator Allocator = std::allocator<vk::SurfaceFormatKHR>>
        std::vector<vk::SurfaceFormatKHR, Allocator> get_surface_formats(const vk::SurfaceKHR surface,
                                                                         Allocator allocator = {}) const
        {
            return physical_device_.getSurfaceFormatsKHR(surface, allocator);
        }

        template <SimpleAllocator Allocator = std::allocator<vk::PresentModeKHR>>
        std::vector<vk::PresentModeKHR, Allocator> get_surface_preset_modes(const vk::SurfaceKHR surface,
                                                                            Allocator allocator = {}) const
        {
            return physical_device_.getSurfacePresentModesKHR(surface, allocator);
        }

        VulkanBufferManager create_buffer_manager(std::size_t pool_size = default_pool_size) const;

        VulkanStagingBuffer create_staging_buffer(vk::DeviceSize size) const;

        std::uint32_t find_memory_type(std::uint32_t type_filter, vk::MemoryPropertyFlags properties) const;

        vk::UniqueCommandPool create_command_pool() const;

        template <SimpleAllocator Allocator = std::allocator<vk::UniqueCommandBuffer>>
        inline std::vector<vk::UniqueCommandBuffer, Allocator> create_command_buffers(
            const vk::CommandBufferAllocateInfo &allocate_info,
            Allocator allocator = {}) const
        {
            return device_->allocateCommandBuffersUnique(allocate_info, allocator);
        }

        inline vk::UniqueSwapchainKHR create_swapchain(const vk::SwapchainCreateInfoKHR &swapchain_create_info)
        {
            return device_->createSwapchainKHRUnique(swapchain_create_info);
        }

        template <SimpleAllocator Allocator = std::allocator<vk::Image>>
        std::vector<vk::Image, Allocator> get_swapchain_images(vk::SwapchainKHR swapchain, Allocator allocator = {})
        {
            return device_->getSwapchainImagesKHR(swapchain, allocator);
        }

        inline auto acquire_next_image(vk::SwapchainKHR swapchain,
                                       std::uint64_t timeout,
                                       vk::Semaphore semaphore,
                                       vk::Fence fence,
                                       std::uint32_t &image_index)
        {
            return device_->acquireNextImageKHR(swapchain, timeout, semaphore, fence, &image_index);
        }

        vk::UniqueSampler create_linear_sampler() const;

        inline vk::UniqueDescriptorPool create_descriptor_pool(const vk::DescriptorPoolCreateInfo &create_info) const
        {
            return device_->createDescriptorPoolUnique(create_info);
        }

        inline void reset_descriptor_pool(const vk::DescriptorPool pool)
        {
            device_->resetDescriptorPool(pool);
        }

        inline vk::UniqueDescriptorSetLayout create_descriptor_set_layout(
            const vk::DescriptorSetLayoutCreateInfo &create_info) const
        {
            return device_->createDescriptorSetLayoutUnique(create_info);
        }

        template <SimpleAllocator Allocator = std::allocator<vk::DescriptorSet>>
        std::vector<vk::DescriptorSet, Allocator> create_descriptor_sets(const vk::DescriptorPool pool,
                                                                         const std::uint32_t count,
                                                                         const vk::DescriptorSetLayout layout)
        {
            return device_->allocateDescriptorSets(vk::DescriptorSetAllocateInfo{.descriptorPool = pool,
                                                                                 .descriptorSetCount = count,
                                                                                 .pSetLayouts = &layout});
        }

        void update_descriptor_sets(const std::span<const vk::WriteDescriptorSet> write_descriptor_sets,
                                    std::span<const vk::CopyDescriptorSet> copy_descriptor_sets = {})
        {
            device_->updateDescriptorSets(write_descriptor_sets, copy_descriptor_sets);
        }

        inline vk::UniqueImage create_image(const vk::ImageCreateInfo &create_info) const
        {
            return device_->createImageUnique(create_info);
        }

        inline vk::UniqueImageView create_image_view(const vk::ImageViewCreateInfo &create_info) const
        {
            return device_->createImageViewUnique(create_info);
        }

        vk::UniqueDeviceMemory allocate_image_memory(vk::Image image) const;

        vk::UniqueRenderPass create_render_pass(const vk::RenderPassCreateInfo &info)
        {
            return device_->createRenderPassUnique(info);
        }

        vk::UniqueFramebuffer create_framebuffer(const vk::FramebufferCreateInfo &info)
        {
            return device_->createFramebufferUnique(info);
        }

        vk::UniqueSemaphore create_semaphore(const vk::SemaphoreCreateInfo &info = {}) const
        {
            return device_->createSemaphoreUnique(info);
        }

        inline vk::UniqueFence create_fence(const vk::FenceCreateInfo &create_info = {}) const
        {
            return device_->createFenceUnique(create_info);
        }

        inline void wait_for_fences(const vk::Fence fence,
                                    const bool wait_all = true,
                                    const std::uint64_t timeout = std::numeric_limits<std::uint64_t>::max()) const
        {
            wait_for_fences(std::span{&fence, 1}, wait_all, timeout);
        }

        inline void wait_for_fences(const std::span<const vk::Fence> fences,
                                    const bool wait_all = true,
                                    const std::uint64_t timeout = std::numeric_limits<std::uint64_t>::max()) const
        {
            if (device_->waitForFences(fences.size(), fences.data(), wait_all, timeout) != vk::Result::eSuccess)
            {
                throw GraphicsException{"VulkanRenderer2D: failed waiting for fences"};
            }
        }

        inline void reset_fences(const vk::Fence fence) const
        {
            reset_fences(std::span{&fence, 1});
        }

        inline void reset_fences(const std::span<const vk::Fence> fences) const
        {
            if (const auto result = device_->resetFences(fences.size(), fences.data()); result != vk::Result::eSuccess)
            {
                throw GraphicsException{"VulkanRenderer2D: failed resetting fences"};
            }
        }

        inline void wait_idle() const
        {
            device_->waitIdle();
        }

      private:
        vk::PhysicalDevice physical_device_{};
        vk::UniqueDevice device_{};
        std::uint32_t graphics_family_index_{std::numeric_limits<std::uint32_t>::max()};
        std::uint32_t present_family_index_{std::numeric_limits<std::uint32_t>::max()};
        vk::Queue graphics_queue_{};
        vk::Queue present_queue_{};

        mutable std::mutex graphics_queue_mutex_;
        mutable std::mutex present_queue_mutex_;
    };
} // namespace retro
