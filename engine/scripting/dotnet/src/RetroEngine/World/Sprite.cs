// // @file Sprite.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Assets;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;

namespace RetroEngine.World;

public readonly record struct UVs(Vector2F Min, Vector2F Max);

public partial class Sprite : SceneObject
{
    public Texture? Texture
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetTexture(NativeObject, value?.NativeObject ?? IntPtr.Zero);
            if (value is null)
                return;

            var uvXRange = UVs.Max.X - UVs.Min.X;
            var uvYRange = UVs.Max.Y - UVs.Min.Y;
            Size = new Vector2F(value.Width * uvXRange, value.Height * uvYRange);
        }
    }

    public Vector2F Size
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetSize(NativeObject, value);
        }
    }

    public Color Tint
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetTint(NativeObject, value);
        }
    }

    public Vector2F Pivot
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetPivot(NativeObject, value);
        }
    }

    public UVs UVs
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetUVs(NativeObject, value);
        }
    }

    private Sprite(Scene scene, IntPtr parent)
        : base(scene, NativeCreate(scene.NativeHandle, parent))
    {
        Size = new Vector2F(100, 100);
        Tint = new Color(1, 1, 1);
        UVs = new UVs(Vector2F.Zero, Vector2F.One);
    }

    public Sprite(Scene scene)
        : this(scene, IntPtr.Zero) { }

    public Sprite(SceneObject parent)
        : this(parent.Scene, parent.NativeObject) { }

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_create")]
    private static partial IntPtr NativeCreate(IntPtr scene, IntPtr id);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_texture")]
    private static partial void NativeSetTexture(IntPtr id, IntPtr texture);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_tint")]
    private static partial void NativeSetTint(IntPtr id, Color color);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_pivot")]
    private static partial void NativeSetPivot(IntPtr id, Vector2F pivot);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_size")]
    private static partial void NativeSetSize(IntPtr id, Vector2F size);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_uv_rect")]
    private static partial void NativeSetUVs(IntPtr id, UVs size);
}
