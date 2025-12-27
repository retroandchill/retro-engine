//
// Created by fcors on 12/26/2025.
//
module;

#include <SDL3/SDL_vulkan.h>
#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(Window window)
        : window_{std::move(window)}, instance_{get_instance_create_info()}, surface_{instance_, window_},
          device_{instance_, surface_.surface()}, swapchain_(SwapchainConfig{
                                                      .physical_device = device_.physical_device(),
                                                      .device = device_.device(),
                                                      .surface = surface_.surface(),
                                                      .graphics_family = device_.graphics_family_index(),
                                                      .present_family = device_.present_family_index(),
                                                      .width = static_cast<uint32_t>(window_.width()),
                                                      .height = static_cast<uint32_t>(window_.height()),
                                                  }),
          render_pass_(RenderPassConfig{
              .device = device_.device(),
              .color_format = swapchain_.format(),
          }),
          framebuffers_(FramebufferConfig{
              .device = device_.device(),
              .render_pass = render_pass_.handle(),
              .extent = swapchain_.extent(),
              .image_views = &swapchain_.image_views(),
          }),
          command_pool_(CommandPoolConfig{
              .device = device_.device(),
              .queue_family_idx = device_.graphics_family_index(),
              .buffer_count = MAX_FRAMES_IN_FLIGHT,
          }),
          sync_(SyncConfig{
              .device = device_.device(),
              .frames_in_flight = MAX_FRAMES_IN_FLIGHT,
          })
    {
    }

    VulkanRenderer2D::~VulkanRenderer2D()
    {
        if (device_.device() != VK_NULL_HANDLE)
        {
            vkDeviceWaitIdle(device_.device());
        }
    }

    void VulkanRenderer2D::begin_frame()
    {
    }

    void VulkanRenderer2D::end_frame()
    {
        VkDevice dev = device_.device();

        VkFence in_flight = sync_.in_flight(current_frame_);
        vkWaitForFences(dev, 1, &in_flight, VK_TRUE, UINT64_MAX);
        vkResetFences(dev, 1, &in_flight);

        uint32 image_index = 0;
        VkResult result = vkAcquireNextImageKHR(dev,
                                                swapchain_.handle(),
                                                UINT64_MAX,
                                                sync_.image_available(current_frame_),
                                                VK_NULL_HANDLE,
                                                &image_index);

        if (result == VK_ERROR_OUT_OF_DATE_KHR)
        {
            recreate_swapchain();
            return;
        }
        else if (result != VK_SUCCESS && result != VK_SUBOPTIMAL_KHR)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to acquire swapchain image"};
        }

        VkCommandBuffer cmd = command_pool_.buffer_at(current_frame_);
        vkResetCommandBuffer(cmd, 0);
        record_command_buffer(cmd, image_index);

        VkSemaphore wait_semaphores[] = {sync_.image_available(current_frame_)};
        VkPipelineStageFlags wait_stages[] = {VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT};
        VkSemaphore signal_semaphores[] = {sync_.render_finished(current_frame_)};

        VkSubmitInfo submit_info{};
        submit_info.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
        submit_info.waitSemaphoreCount = 1;
        submit_info.pWaitSemaphores = wait_semaphores;
        submit_info.pWaitDstStageMask = wait_stages;
        submit_info.commandBufferCount = 1;
        submit_info.pCommandBuffers = &cmd;
        submit_info.signalSemaphoreCount = 1;
        submit_info.pSignalSemaphores = signal_semaphores;

        if (vkQueueSubmit(device_.graphics_queue(), 1, &submit_info, in_flight) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
        }

        VkSwapchainKHR swapchains[] = {swapchain_.handle()};

        VkPresentInfoKHR present_info{};
        present_info.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
        present_info.waitSemaphoreCount = 1;
        present_info.pWaitSemaphores = signal_semaphores;
        present_info.swapchainCount = 1;
        present_info.pSwapchains = swapchains;
        present_info.pImageIndices = &image_index;

        result = vkQueuePresentKHR(device_.present_queue(), &present_info);

        if (result == VK_ERROR_OUT_OF_DATE_KHR || result == VK_SUBOPTIMAL_KHR)
        {
            recreate_swapchain();
        }
        else if (result != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to present swapchain image"};
        }

        current_frame_ = (current_frame_ + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    void VulkanRenderer2D::draw_quad(Vector2 position, Vector2 size, Color color)
    {
        // Placeholder: real drawing will come once the pipeline is set up.
        (void)position;
        (void)size;
        (void)color;
    }

    VulkanInstance VulkanRenderer2D::get_instance_create_info()
    {
        VkApplicationInfo app_info{.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO,
                                   .pApplicationName = "Retro Engine",
                                   .applicationVersion = VK_MAKE_VERSION(1, 0, 0),
                                   .pEngineName = "Retro Engine",
                                   .engineVersion = VK_MAKE_VERSION(1, 0, 0),
                                   .apiVersion = VK_API_VERSION_1_2};

        const auto extensions = get_required_instance_extensions();

        const VkInstanceCreateInfo create_info{.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
                                               .pApplicationInfo = &app_info,
                                               .enabledExtensionCount = static_cast<uint32>(extensions.size()),
                                               .ppEnabledExtensionNames = extensions.data()};

        return VulkanInstance{create_info};
    }

    std::span<const char *const> VulkanRenderer2D::get_required_instance_extensions()
    {
        // Ask SDL what Vulkan instance extensions are required for this window
        uint32 count = 0;
        auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
        if (names == nullptr)
        {
            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
        }

        return std::span{names, count};
    }

    void VulkanRenderer2D::recreate_swapchain()
    {
        // Query new size from window_
        const auto w = static_cast<uint32>(window_.width());
        const auto h = static_cast<uint32>(window_.height());
        if (w == 0 || h == 0)
            return;

        vkDeviceWaitIdle(device_.device());

        swapchain_.recreate(SwapchainConfig{
            .physical_device = device_.physical_device(),
            .device = device_.device(),
            .surface = surface_.surface(),
            .graphics_family = device_.graphics_family_index(),
            .present_family = device_.present_family_index(),
            .width = w,
            .height = h,
        });

        render_pass_.recreate(RenderPassConfig{
            .device = device_.device(),
            .color_format = swapchain_.format(),
        });

        framebuffers_.recreate(FramebufferConfig{
            .device = device_.device(),
            .render_pass = render_pass_.handle(),
            .extent = swapchain_.extent(),
            .image_views = &swapchain_.image_views(),
        });
    }

    void VulkanRenderer2D::record_command_buffer(VkCommandBuffer cmd, uint32 image_index)
    {
        VkCommandBufferBeginInfo begin_info{};
        begin_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

        if (vkBeginCommandBuffer(cmd, &begin_info) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to begin command buffer"};
        }

        VkClearValue clear{};
        clear.color = {{0.1f, 0.1f, 0.2f, 1.0f}};

        VkRenderPassBeginInfo rp_info{};
        rp_info.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
        rp_info.renderPass = render_pass_.handle();
        rp_info.framebuffer = framebuffers_.framebuffer_at(image_index);
        rp_info.renderArea.offset = {0, 0};
        rp_info.renderArea.extent = swapchain_.extent();
        rp_info.clearValueCount = 1;
        rp_info.pClearValues = &clear;

        vkCmdBeginRenderPass(cmd, &rp_info, VK_SUBPASS_CONTENTS_INLINE);

        // TODO: issue pipeline & draw commands here for your quad

        vkCmdEndRenderPass(cmd);

        if (vkEndCommandBuffer(cmd) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to record command buffer"};
        }
    }
} // namespace retro
