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
    concept RenderProxy = PackableType<T> && requires(const T &t) {
        typename T::DrawCallData;
        {
            T::type_id()
        } -> std::convertible_to<Name>;
        {
            t.get_draw_call()
        } -> std::convertible_to<typename T::DrawCallData>;
    };

    export class RenderProxyBucket
    {
      public:
        virtual ~RenderProxyBucket() = default;

        virtual Name type_id() const = 0;

        virtual std::any collect_draw_calls() const = 0;
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

        std::any collect_draw_calls() const override
        {
            return proxies_ | std::views::transform([](const auto &proxy) { return proxy.get_draw_call(); }) |
                   std::ranges::to<std::vector>();
        }

      private:
        PackedPool<T> proxies_;
    };
} // namespace retro
