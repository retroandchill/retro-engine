//
// Created by fcors on 12/31/2025.
//

export module retro.runtime:scene.rendering.render_proxy_bucket;

import std;
import retro.core;

namespace retro
{
    template <typename T>
    concept RenderProxy = requires(const T &t) {
        typename T::DrawCallData;
        { T::type_id() } -> std::convertible_to<Name>;
        { t.id() } -> std::convertible_to<uint64>;
        { t.get_draw_call() } -> std::convertible_to<typename T::DrawCallData>;
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

        uint64 add(T proxy)
        {
            auto id = proxy.id;
            proxies_.push_back(std::move(proxy));
            proxy_indices_.emplace(id, proxies_.size() - 1);
            return id;
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        uint64 emplace(Args &&...args)
        {
            auto &added = proxies_.emplace_back(std::forward<Args>(args)...);
            proxy_indices_.emplace(added.id(), proxies_.size() - 1);
            return added.id();
        }

        void remove(const uint64 id)
        {
            if (const auto index = proxy_indices_.find(id); index != proxy_indices_.end())
            {
                proxies_.erase(proxies_.begin() + index->second);
                proxy_indices_.erase(index);
            }
        }

        std::any collect_draw_calls() const override
        {
            return proxies_ | std::views::transform([](const auto &proxy) { return proxy.get_draw_call(); }) |
                   std::ranges::to<std::vector>();
        }

      private:
        std::vector<T> proxies_;
        std::map<uint64, usize> proxy_indices_;
    };
} // namespace retro