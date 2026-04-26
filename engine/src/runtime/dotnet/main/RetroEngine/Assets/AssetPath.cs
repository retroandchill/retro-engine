// // @file AssetPath.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

[StructLayout(LayoutKind.Sequential)]
public readonly struct AssetPath : IEquatable<AssetPath>, IEqualityOperators<AssetPath, AssetPath, bool>
{
    public const char PackageSeparator = ':';

    public Name PackageName { get; }

    public Name AssetName { get; }

    public bool IsValid =>
        PackageName is { IsValid: true, IsNone: false } && AssetName is { IsValid: true, IsNone: false };

    public static AssetPath None => new(Name.None, Name.None);

    public AssetPath(Name packageName, Name assetName)
    {
        PackageName = packageName;
        AssetName = assetName;
    }

    public AssetPath(ReadOnlySpan<char> path)
    {
        var segments = 0;
        foreach (var range in path.Split(PackageSeparator))
        {
            var segment = path[range];
            segments++;

            switch (segments)
            {
                case 1:
                    PackageName = new Name(segment);
                    break;
                case 2:
                    AssetName = new Name(segment);
                    break;
                default:
                    throw new ArgumentException("Invalid asset path", nameof(path));
            }
        }

        if (segments != 2)
        {
            throw new ArgumentException("Invalid asset path", nameof(path));
        }
    }

    public override string ToString()
    {
        if (!IsValid)
        {
            return string.Empty;
        }

        var maxLength = Encoding.UTF8.GetMaxCharCount(Name.MaxRenderedLength * 2 + 1);
        Span<char> buffer = stackalloc char[maxLength];
        var writtenLength = PackageName.WriteUtf16Bytes(buffer);
        buffer[writtenLength] = PackageSeparator;
        writtenLength++;
        writtenLength += AssetName.WriteUtf16Bytes(buffer[writtenLength..]);
        return buffer[..writtenLength].ToString();
    }

    public bool Equals(AssetPath other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        return obj is AssetPath other && Equals(other);
    }

    public static bool operator ==(AssetPath left, AssetPath right)
    {
        return left.PackageName == right.PackageName && left.AssetName == right.AssetName;
    }

    public static bool operator !=(AssetPath left, AssetPath right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PackageName, AssetName);
    }
}
