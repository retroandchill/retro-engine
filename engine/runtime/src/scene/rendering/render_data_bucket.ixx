//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.render_data_bucket;

import std;
import retro.core;

namespace retro
{
    export struct RenderDataBatch
    {
        Name type;
        const void *data;
        usize count;
        usize stride;
    };

    export class RenderDataBucket
    {
      public:
        virtual ~RenderDataBucket() = default;

        virtual usize size() const = 0;
        virtual bool dirty() const = 0;
        virtual void clear_dirty() = 0;

        virtual RenderDataBatch build_batch() const = 0;

        virtual uint64 add_raw(const void *proxy) = 0;
        virtual void update_raw(uint64 id, const void *proxy) = 0;
        virtual void remove_raw(uint64 id) = 0;
    };

    template <typename Data>
        requires(std::is_trivially_destructible_v<Data> && std::is_trivially_copyable_v<Data>)
    class RenderDataBucketImpl final : RenderDataBucket
    {
      public:
        inline explicit RenderDataBucketImpl(const Name type) : type_(type)
        {
        }

        uint64 add_raw(const void *proxy) override
        {
            return add(*static_cast<const Data *>(proxy));
        }

        uint64 add(const Data &proxy)
        {
            uint64 id;
            if (!free_list_.empty())
            {
                id = free_list_.back();
                free_list_.pop_back();
                data_[id] = proxy;
            }
            else
            {
                id = data_.size();
                data_.emplace_back(proxy);
            }
            dirty_ = true;
            return id;
        }

        void update_raw(const uint64 id, const void *proxy) override
        {
            update(id, *static_cast<const Data *>(proxy));
        }

        void update(uint64 id, const Data &proxy)
        {
            data_[id] = proxy;
            dirty_ = true;
        }

        void remove_raw(const uint64 id) override
        {
            remove(id);
        }

        void remove(const uint64 id)
        {
            free_list_.push_back(id);
            dirty_ = true;
        }

        [[nodiscard]] RenderDataBatch build_batch() const override
        {
            return RenderDataBatch{.type = type_, .data = data_.data(), .count = data_.size(), .stride = sizeof(Data)};
        }
        [[nodiscard]] std::span<const Data> data() const
        {
            return data_;
        }
        [[nodiscard]] usize size() const override
        {
            return data_.size();
        }

        [[nodiscard]] bool dirty() const override
        {
            return dirty_;
        }
        void clear_dirty() override
        {
            dirty_ = false;
        }

      private:
        Name type_;
        std::vector<Data> data_;
        std::vector<uint64> free_list_;
        bool dirty_ = false;
    };
} // namespace retro