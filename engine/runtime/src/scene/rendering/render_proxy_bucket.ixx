/**
 * @file render_proxy_bucket.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.rendering.render_proxy_bucket;

import std;
import retro.core;

namespace retro
{
    export using RenderProxyID = DefaultHandle;

    template <typename T>
    concept RenderProxy = PackableType<T> && requires(const T &t, Vector2u viewport_size) {
        typename T::DrawCallData;
        {
            T::type_id()
        } -> std::convertible_to<Name>;
        {
            t.get_draw_call(viewport_size)
        } -> std::convertible_to<typename T::DrawCallData>;
    };

    export class RenderProxyBucket
    {
      public:
        virtual ~RenderProxyBucket() = default;

        [[nodiscard]] virtual Name type_id() const = 0;

        [[nodiscard]] virtual std::any collect_draw_calls(Vector2u viewport_size) const = 0;
    };

    template <RenderProxy T>
    class RenderProxyBucketImpl final : public RenderProxyBucket
    {
      public:
        Name type_id() const override
        {
            return T::type_id();
        }

        template <typename... Args>
            requires std::constructible_from<T, RenderProxyID, Args...>
        RenderProxyID emplace(Args &&...args)
        {
            auto &added = proxies_.emplace(std::forward<Args>(args)...);
            return added.id();
        }

        void remove(const RenderProxyID id)
        {
            proxies_.remove(id);
        }

        std::any collect_draw_calls(const Vector2u viewport_size) const override
        {
            return proxies_ |
                   std::views::transform([viewport_size](const auto &proxy)
                                         { return proxy.get_draw_call(viewport_size); }) |
                   std::ranges::to<std::vector>();
        }

      private:
        PackedPool<T> proxies_;
    };
} // namespace retro
