//
// Created by fcors on 12/31/2025.
//

export module retro.runtime:scene.rendering.render_proxy_manager;

import std;
import retro.core;
import :scene.rendering.render_proxy_bucket;

namespace retro
{
    export class RenderProxyManager
    {
      public:
        template <typename T>
            requires RenderProxy<std::remove_cvref_t<T>> && std::constructible_from<std::remove_cvref_t<T>, T>
        uint64 add_proxy(T &&proxy)
        {
            using ProxyType = std::remove_cvref_t<T>;

            auto existing = proxy_indices_.find(ProxyType::type_id);
            if (existing != proxy_indices_.end())
            {
                auto proxyBucket = static_cast<RenderProxyBucketImpl<T> &>(*buckets_[*existing]);
                return proxyBucket.emplace(std::forward<T>(proxy));
            }

            auto new_bucket = std::make_unique<RenderProxyBucketImpl<ProxyType>>();
            auto id = new_bucket->add(std::forward<T>(proxy));
            buckets_.emplace(std::move(new_bucket));
            proxy_indices_.emplace(ProxyType::type_id, buckets_.size() - 1);
            return id;
        }

        template <RenderProxy T, typename... Args>
            requires std::constructible_from<T, Args...>
        uint64 emplace_proxy(Args &&...args)
        {
            auto existing = proxy_indices_.find(T::type_id);
            if (existing != proxy_indices_.end())
            {
                auto proxyBucket = static_cast<RenderProxyBucketImpl<T> &>(*buckets_[*existing]);
                return proxyBucket.emplace(std::forward<Args>(args)...);
            }

            auto new_bucket = std::make_unique<RenderProxyBucketImpl<T>>();
            auto id = new_bucket->emplace(std::forward<Args>(args)...);
            buckets_.emplace(std::move(new_bucket));
            proxy_indices_.emplace(T::type_id, buckets_.size() - 1);
            return id;
        }

      private:
        std::vector<std::unique_ptr<RenderProxyBucket>> buckets_;
        std::map<Name, uint64> proxy_indices_;
    };
} // namespace retro