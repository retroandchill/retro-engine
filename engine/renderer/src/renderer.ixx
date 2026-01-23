/**
 * @file renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer;

export import :components;
export import :pipeline;
import retro.core;
import vulkan_hpp;
import std;
import sdl;
import boost;

namespace retro
{
    export class VulkanViewport
    {
      public:
        virtual ~VulkanViewport() = default;

        virtual vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const = 0;

        [[nodiscard]] virtual Vector2u size() const = 0;

        [[nodiscard]] inline uint32 width() const
        {
            return size().x;
        }

        [[nodiscard]] inline uint32 height() const
        {
            return size().y;
        }
    };

    struct WindowDeleter
    {
        inline void operator()(sdl::SDL_Window *window) const noexcept
        {
            sdl::DestroyWindow(window);
        }
    };

    using WindowPtr = std::unique_ptr<sdl::SDL_Window, WindowDeleter>;

    export class RETRO_API Window final : public VulkanViewport
    {
      public:
        inline Window(const int32 width, const int32 height, const CStringView title)
        {
            window_ = WindowPtr{
                sdl::CreateWindow(title.data(), width, height, sdl::WindowFlags::RESIZABLE | sdl::WindowFlags::VULKAN),
            };

            if (window_ == nullptr)
            {
                throw std::runtime_error{std::string{"SDL_CreateWindow failed: "} + sdl::GetError()};
            }
        }

        [[nodiscard]] inline sdl::SDL_Window *native_handle() const
        {
            return window_.get();
        }

        // NOLINTNEXTLINE
        inline void set_title(const CStringView title)
        {
            sdl::SetWindowTitle(window_.get(), title.data());
        }

        [[nodiscard]] vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const override;

        [[nodiscard]] inline Vector2u size() const override
        {
            int w = 0;
            int h = 0;
            sdl::GetWindowSizeInPixels(window_.get(), &w, &h);
            return {static_cast<uint32>(w), static_cast<uint32>(h)};
        }

        [[nodiscard]] inline friend bool operator==(const Window &a, const Window &b) noexcept
        {
            return a.native_handle() == b.native_handle();
        }

        [[nodiscard]] inline friend bool operator==(const Window &a, std::nullptr_t) noexcept
        {
            return a.native_handle() == nullptr;
        }

      private:
        WindowPtr window_;
    };

    export struct TransientAllocation
    {
        vk::Buffer buffer;
        void *mapped_data;
        size_t offset;
    };

    export class RETRO_API VulkanBufferManager
    {
        explicit VulkanBufferManager(const VulkanDevice &device, usize pool_size);

      public:
        static void initialize(const VulkanDevice &device, usize pool_size = PipelineManager::DEFAULT_POOL_SIZE);

        static void shutdown();

        static VulkanBufferManager &instance();

        TransientAllocation allocate_transient(usize size, vk::BufferUsageFlags usage);

        void reset();

      private:
        [[nodiscard]] uint32 find_memory_type(uint32 type_filter, vk::MemoryPropertyFlags properties) const;

        vk::PhysicalDevice physical_device_;
        vk::Device device_;
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        void *mapped_ptr_ = nullptr;
        usize pool_size_{PipelineManager::DEFAULT_POOL_SIZE};
        usize current_offset_ = 0;

        static std::unique_ptr<VulkanBufferManager> instance_;
    };

    export class RETRO_API VulkanBufferManagerScope
    {
      public:
        explicit inline VulkanBufferManagerScope(const VulkanDevice &device,
                                                 const usize pool_size = PipelineManager::DEFAULT_POOL_SIZE)
        {
            VulkanBufferManager::initialize(device, pool_size);
        }

        VulkanBufferManagerScope(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope(VulkanBufferManagerScope &&) noexcept = delete;

        inline ~VulkanBufferManagerScope()
        {
            VulkanBufferManager::shutdown();
        }

        VulkanBufferManagerScope &operator=(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope &operator=(VulkanBufferManagerScope &&) noexcept = delete;
    };

    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        explicit VulkanRenderer2D(std::shared_ptr<VulkanViewport> viewport);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        ~VulkanRenderer2D() override;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;
        void begin_frame() override;

        void end_frame() override;

        [[nodiscard]] Vector2u viewport_size() const override;

        void add_new_render_pipeline(std::type_index type, std::shared_ptr<RenderPipeline> pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

      private:
        static vk::UniqueInstance create_instance();
        static std::span<const char *const> get_required_instance_extensions();
        static vk::UniqueRenderPass create_render_pass(vk::Device device,
                                                       vk::Format color_format,
                                                       vk::SampleCountFlagBits samples);
        static std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
                                                                      vk::RenderPass render_pass,
                                                                      const VulkanSwapchain &swapchain);

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, uint32 image_index);

      private:
        std::shared_ptr<VulkanViewport> viewport_;

        vk::UniqueInstance instance_;
        vk::UniqueSurfaceKHR surface_;
        VulkanDevice device_;
        VulkanBufferManagerScope buffer_manager_;
        VulkanSwapchain swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        VulkanCommandPool command_pool_;
        VulkanSyncObjects sync_;
        VulkanPipelineManager pipeline_manager_;

        uint32 current_frame_ = 0;
        uint32 image_index_ = 0;

        static constexpr uint32 MAX_FRAMES_IN_FLIGHT = 2;
    };

    export inline auto make_rendering_injector(std::shared_ptr<VulkanViewport> viewport)
    {
        return boost::di::make_injector(boost::di::bind<VulkanViewport>().to(std::move(viewport)),
                                        boost::di::bind<Renderer2D>().to<VulkanRenderer2D>());
    }
} // namespace retro
