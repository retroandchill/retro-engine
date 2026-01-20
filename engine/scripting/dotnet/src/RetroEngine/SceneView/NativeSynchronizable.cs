// @file NativeSynchronizable.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;

namespace RetroEngine.SceneView;

public interface INativeSynchronizable
{
    uint Id { get; }
}

public interface ITransformSync : INativeSynchronizable
{
    Vector2F Position { get; }
    float Rotation { get; }
    Vector2F Scale { get; }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TransformUpdate
{
    public uint Id { get; init; }
    public Vector2F Position { get; init; }
    public float Rotation { get; init; }
    public Vector2F Scale { get; init; }
}

public interface IViewSync : INativeSynchronizable
{
    Vector2F Size { get; }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct ViewUpdate
{
    public uint Id { get; init; }
    public Vector2F Size { get; init; }
}

public interface IGeometrySync : INativeSynchronizable
{
    void SyncGeometry(Action<uint, ReadOnlySpan<Vertex>, ReadOnlySpan<uint>> syncCallback);
}
