// // @file AssetPath.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct AssetPath : IEquatable<AssetPath>, IEqualityOperators<AssetPath, AssetPath, bool>
{
    public Name PackageName { get; }

    public Name AssetName { get; }

    public bool IsValid => NativeIsValid(this);

    public static AssetPath None => new(Name.None, Name.None);

    public AssetPath(Name packageName, Name assetName)
    {
        PackageName = packageName;
        AssetName = assetName;
    }

    public AssetPath(ReadOnlySpan<char> path)
    {
        if (!NativeFromString(path, out this))
        {
            throw new ArgumentException($"'{path}' is not a valid asset path.", nameof(path));
        }
    }

    public override string ToString()
    {
        Span<char> buffer = stackalloc char[Name.MaxLength * 2 + 1];
        var newLength = NativeToString(in this, buffer);
        return buffer[..newLength].ToString();
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

    [LibraryImport("retro_runtime", EntryPoint = "retro_asset_path_from_string")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeFromString(ReadOnlySpan<char> path, int length, out AssetPath assetPath);

    private static bool NativeFromString(ReadOnlySpan<char> path, out AssetPath assetPath) =>
        NativeFromString(path, path.Length, out assetPath);

    [LibraryImport("retro_runtime", EntryPoint = "retro_asset_path_is_valid")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsValid(in AssetPath path);

    [LibraryImport("retro_runtime", EntryPoint = "retro_asset_path_to_string")]
    private static partial int NativeToString(in AssetPath path, Span<char> buffer, int bufferLength);

    private static int NativeToString(in AssetPath path, Span<char> buffer) =>
        NativeToString(in path, buffer, buffer.Length);
}
