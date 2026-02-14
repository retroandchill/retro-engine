/**
 * @file service_identifier.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_identifier;

import std;
import retro.core.algorithm.hashing;

namespace retro
{
    struct ServiceIdentifier
    {
        std::type_index type;

        explicit inline ServiceIdentifier(const std::type_info &type) noexcept : type(type)
        {
        }

        explicit inline ServiceIdentifier(const std::type_index &type) noexcept : type(type)
        {
        }

        friend bool operator==(const ServiceIdentifier &lhs, const ServiceIdentifier &rhs) noexcept = default;
    };

    struct ServiceCacheKey
    {
        ServiceIdentifier id = ServiceIdentifier{typeid(void)};
        std::uint32_t slot{};

        friend bool operator==(const ServiceCacheKey &lhs, const ServiceCacheKey &rhs) noexcept = default;
    };
} // namespace retro

template <>
struct std::hash<retro::ServiceIdentifier>
{
    [[nodiscard]] inline size_t operator()(const retro::ServiceIdentifier &id) const noexcept
    {
        return id.type.hash_code();
    }
};

template <>
struct std::hash<retro::ServiceCacheKey>
{
    [[nodiscard]] inline size_t operator()(const retro::ServiceCacheKey &key) const noexcept
    {
        return retro::hash_combine(key.id, key.slot);
    }
};
