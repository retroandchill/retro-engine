/**
 * @file render_proxy_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_proxy_manager;

import std;
import retro.core;
import :scene.rendering.render_proxy_bucket;

namespace retro
{
    export class RETRO_API RenderProxyManager
    {
      public:
        RenderProxyManager() = default;
        ~RenderProxyManager() = default;

        RenderProxyManager(const RenderProxyManager &) = delete;
        RenderProxyManager(RenderProxyManager &&) = default;
        RenderProxyManager &operator=(const RenderProxyManager &) = delete;
        RenderProxyManager &operator=(RenderProxyManager &&) = default;

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
            buckets_.emplace_back(std::move(new_bucket));
            proxy_indices_.emplace(ProxyType::type_id, buckets_.size() - 1);
            return id;
        }

        template <RenderProxy T, typename... Args>
            requires std::constructible_from<T, RenderProxyID, Args...>
        RenderProxyID emplace_proxy(Args &&...args)
        {
            auto existing = proxy_indices_.find(T::type_id());
            if (existing != proxy_indices_.end())
            {
                auto &proxyBucket = static_cast<RenderProxyBucketImpl<T> &>(*buckets_[existing->second]);
                return proxyBucket.emplace(std::forward<Args>(args)...);
            }

            auto new_bucket = std::make_unique<RenderProxyBucketImpl<T>>();
            auto id = new_bucket->emplace(std::forward<Args>(args)...);
            buckets_.emplace_back(std::move(new_bucket));
            proxy_indices_.emplace(T::type_id(), buckets_.size() - 1);
            return id;
        }

        template <RenderProxy T>
        void remove_proxy(const RenderProxyID id)
        {
            auto existing = proxy_indices_.find(T::type_id());
            if (existing != proxy_indices_.end())
            {
                auto proxyBucket = static_cast<RenderProxyBucketImpl<T> &>(*buckets_[existing->second]);
                return proxyBucket.remove(id);
            }
        }

        auto collect_draw_calls() const
        {
            return buckets_ |
                   std::views::transform([](const std::unique_ptr<RenderProxyBucket> &bucket)
                                         { return std::make_pair(bucket->type_id(), bucket->collect_draw_calls()); });
        }

      private:
        std::vector<std::unique_ptr<RenderProxyBucket>> buckets_;
        std::map<Name, usize> proxy_indices_;
    };
} // namespace retro
