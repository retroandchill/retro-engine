// @file LocalizedType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Namotion.Reflection;
using RetroEngine.Portable.Localization;
using RetroEngine.Utilities;

namespace RetroEngine.Localization.Reflection;

public sealed class LocalizedType : IEquatable<LocalizedType>, IEqualityOperators<LocalizedType, LocalizedType, bool>
{
    private static readonly ConditionalWeakTable<Type, LocalizedType> TypeCache = new();

    public Type Type { get; }

    public Text DisplayName { get; }

    public Text Description { get; }

    public static LocalizedType Get(Type type)
    {
        return TypeCache.GetOrAdd(type, t => new LocalizedType(t));
    }

    public static LocalizedType Get<T>()
    {
        return Get(typeof(T));
    }

    private LocalizedType(Type type)
    {
        Type = type;
        DisplayName = type.Name.SplitByWords();
        Description = type.ToContextualType().GetXmlDocsSummary();
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is LocalizedType other && Equals(other);
    }

    public bool Equals(LocalizedType? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        return other is not null && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    public static bool operator ==(LocalizedType? left, LocalizedType? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(LocalizedType? left, LocalizedType? right)
    {
        return !(left == right);
    }
}
