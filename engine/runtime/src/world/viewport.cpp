/**
 * @file viewport.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.world.viewport;

namespace retro
{
    Viewport &ViewportManager::create_viewport(const ViewportDescriptor &descriptor)
    {
        const auto &new_viewport = viewports_.emplace_back(std::make_unique<Viewport>(descriptor));
        if (primary_ == nullptr)
        {
            primary_ = new_viewport.get();
        }

        return *new_viewport;
    }

    void ViewportManager::destroy_viewport(Viewport &viewport)
    {
        if (primary_ == std::addressof(viewport))
        {
            primary_ = nullptr;
        }

        const auto it = std::ranges::find_if(viewports_,
                                             [&viewport](const std::unique_ptr<Viewport> &ptr)
                                             { return ptr.get() == std::addressof(viewport); });

        // We could hypothetically swap and pop here, but often there aren't that many viewports,
        // where moving a few pointers down probably isn't going to waste much time.
        if (it != viewports_.end())
        {
            viewports_.erase(it);
        }

        if (primary_ == nullptr && !viewports_.empty())
        {
            primary_ = viewports_.front().get();
        }
    }
} // namespace retro
