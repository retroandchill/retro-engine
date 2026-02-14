/**
 * @file asset.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.assets.asset;

import std;
import retro.runtime.assets.asset_path;
import retro.core.memory.ref_counted_ptr;
import retro.core.strings.name;

namespace retro
{
    export class Asset;

    struct AssetPathHook
    {
        AssetPath path;

        explicit inline AssetPathHook(const AssetPath &path) : path{path}
        {
        }

        AssetPathHook(const AssetPathHook &other) = delete;
        inline AssetPathHook(AssetPathHook &&other) noexcept : path{std::exchange(other.path, AssetPath::none())}
        {
        }

        inline ~AssetPathHook() noexcept
        {
            reset();
        }

        AssetPathHook &operator=(const AssetPathHook &other) = delete;
        inline AssetPathHook &operator=(AssetPathHook &&other) noexcept
        {
            reset();
            path = std::exchange(other.path, AssetPath::none());
            return *this;
        }

        RETRO_API void reset() noexcept;

      private:
        friend class Asset;

        RETRO_API void release() noexcept;
    };

    class Asset : public IntrusiveRefCounted
    {
      protected:
        explicit Asset(const AssetPath &path) : hook_{path}
        {
        }

      public:
        [[nodiscard]] inline AssetPath path() const noexcept
        {
            return hook_.path;
        }

        [[nodiscard]] virtual Name asset_type() const noexcept = 0;

        virtual void on_engine_shutdown();

      private:
        AssetPathHook hook_;
    };
} // namespace retro
