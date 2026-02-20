// // @file CultureId.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace RetroEngine.Portable.Localization.Cultures;

[NativeMarshalling(typeof(CultureIdMarshaller))]
public readonly struct CultureId : IEquatable<CultureId>, IEqualityOperators<CultureId, CultureId, bool>
{
    public string Name { get; }
    internal byte[] Utf8Bytes { get; }

    internal CultureId(string name)
    {
        Name = name;
        Utf8Bytes = Encoding.UTF8.GetBytes(name + '\0');
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is CultureId id && Equals(id);
    }

    public bool Equals(CultureId other)
    {
        return Name.Equals(other.Name, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    public static bool operator ==(CultureId left, CultureId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CultureId left, CultureId right)
    {
        return !(left == right);
    }
}
