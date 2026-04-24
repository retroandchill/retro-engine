/**
 * @file draw_command.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.draw_command;

import std;
import retro.core.containers.inline_list;
import retro.core.memory.small_unique_ptr;
import retro.runtime.world.viewport;
import retro.core.strings.name;
import retro.runtime.rendering.texture;

namespace retro
{
    export constexpr std::size_t draw_array_size = 8;

    export using DescriptorSetData = std::variant<std::span<const std::byte>, const Texture *>;

    export struct DrawCommand
    {
        InlineList<std::span<const std::byte>, draw_array_size> vertex_buffers{};
        InlineList<std::span<const std::byte>, draw_array_size> instance_buffers{};
        std::span<const std::byte> index_buffer;
        InlineList<DescriptorSetData, draw_array_size> descriptor_sets{};
        std::span<const std::byte> push_constants;
        std::size_t index_count{};
        std::size_t instance_count{};
    };

    template <typename T>
    concept DrawCommandData = requires(const T &data) {
        {
            data.create_draw_command()
        } -> std::convertible_to<DrawCommand>;
        typename T::ComponentType;
    };

    export class DrawCommandSource
    {
      public:
        virtual ~DrawCommandSource() = default;

        [[nodiscard]] virtual std::type_index component_type() const noexcept = 0;

        template <DrawCommandData T>
            requires std::same_as<T, std::remove_cvref_t<T>>
        static SmallUniquePtr<DrawCommandSource> from(std::pmr::vector<T> data);

        [[nodiscard]] virtual std::pmr::vector<DrawCommand> get_draw_commands() const noexcept = 0;
    };

    template <DrawCommandData T>
        requires std::same_as<T, std::remove_cvref_t<T>>
    class DrawCommandSourceImpl final : public DrawCommandSource
    {
      public:
        explicit DrawCommandSourceImpl(std::pmr::vector<T> data) : data_(std::move(data))
        {
        }

        [[nodiscard]] std::type_index component_type() const noexcept override
        {
            return typeid(T::ComponentType);
        }

        [[nodiscard]] std::pmr::vector<DrawCommand> get_draw_commands() const noexcept override
        {
            auto *resource = data_.get_allocator().resource();
            return data_ | std::views::transform([](const auto &data) { return data.create_draw_command(); }) |
                   std::ranges::to<std::pmr::vector<DrawCommand>>(resource);
        }

      private:
        std::pmr::vector<T> data_;
    };

    template <DrawCommandData T>
        requires std::same_as<T, std::remove_cvref_t<T>>
    SmallUniquePtr<DrawCommandSource> DrawCommandSource::from(std::pmr::vector<T> data)
    {
        return make_unique_small<DrawCommandSourceImpl<T>>(std::move(data));
    }

    export struct DrawCommandSet
    {
        ScreenLayout layout;
        std::int32_t z_order;
        std::pmr::vector<SmallUniquePtr<DrawCommandSource>> sources;
    };
} // namespace retro
