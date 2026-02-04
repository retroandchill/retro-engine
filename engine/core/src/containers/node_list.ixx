/**
 * @file node_list.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

export module retro.core.containers.node_list;

import std;

namespace retro
{
    export struct NodeHook
    {
        std::size_t master_index = std::dynamic_extent;
        std::size_t internal_index = std::dynamic_extent;
    };

    export template <typename T, auto Member>
    concept HookableNode = requires(T &obj) {
        {
            obj.*Member
        } -> std::same_as<NodeHook &>;
        {
            typeid(obj)
        } -> std::same_as<const std::type_info &>;
    };

    export template <typename T, auto Member>
    class NodeList
    {
      public:
        constexpr NodeList() = default;

        [[nodiscard]] constexpr std::span<const std::unique_ptr<T>> nodes() const noexcept
        {
            return storage_;
        }

        [[nodiscard]] constexpr std::span<T *const> nodes_of_type(const std::type_index type) const noexcept
        {
            const auto it = nodes_by_type_.find(type);
            if (it == nodes_by_type_.end())
            {
                return {};
            }

            return it->second;
        }

        constexpr void add(std::unique_ptr<T> node) noexcept
        {
            index_node(node.get());
            auto &node_member = (*node).*Member;
            node_member.master_index = storage_.size();
            storage_.emplace_back(std::move(node));
        }

        constexpr void remove(T &node) noexcept
        {
            unindex_node(&node);

            assert(node.hook_.master_index < storage_.size());
            auto &existing = storage_[node.hook_.master_index];
            auto &back = storage_.back();
            std::swap(existing, back);
            auto &back_member = (*back).*Member;
            auto &existing_member = node.*Member;
            back_member.master_index = existing_member.master_index;
            storage_.pop_back();
        }

      private:
        constexpr void index_node(T *node) noexcept
        {
            auto &nodes_list = nodes_by_type_[std::type_index{typeid(*node)}];
            nodes_list.push_back(node);
            auto &node_hook = (*node).*Member;
            node_hook.internal_index = nodes_list.size() - 1;
        }

        constexpr void unindex_node(T *node)
        {
            const auto it = nodes_by_type_.find(std::type_index{typeid(*node)});
            if (it == nodes_by_type_.end())
            {
                return;
            }

            auto &vec = it->second;

            auto &node_member = node->*Member;

            assert(node_member.internal_index < vec.size());
            auto &current = vec[node_member.internal_index];
            auto &back = vec.back();
            std::swap(current, back);
            auto &back_member = (*back).*Member;
            back_member.internal_index = node_member.internal_index;
            vec.pop_back();
            node_member.internal_index = std::dynamic_extent;
        }

        std::vector<std::unique_ptr<T>> storage_;
        std::unordered_map<std::type_index, std::vector<T *>> nodes_by_type_{};
    };
} // namespace retro
