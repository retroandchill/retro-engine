//
// Created by fcors on 12/31/2025.
//

export module retro.runtime:scene.rendering.render_proxy_bucket;

import std;
import retro.core;

namespace retro
{
    template <typename T>
    concept RenderProxy = requires(T t) {
        { T::type_id } -> std::convertible_to<Name>;
        { t.id } -> std::convertible_to<uint64>;
    };

    export class RenderProxyBucket
    {
        virtual ~RenderProxyBucket() = default;
    };

    template <RenderProxy T>
    class RenderProxyBucketImpl
    {
      public:
        uint64 add(T proxy)
        {
            auto id = proxy.id;
            proxies_.push_back(std::move(proxy));
            return id;
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        uint64 emplace(Args &&...args)
        {
            auto &added = proxies_.emplace_back(std::forward<Args>(args)...);
            return added.id;
        }

      private:
        std::vector<T> proxies_;
    };
} // namespace retro