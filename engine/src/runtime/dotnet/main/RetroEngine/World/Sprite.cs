// // @file Sprite.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Rendering;

namespace RetroEngine.World;

public enum SpriteDrawMode : byte
{
    Quad,
    Box,
}

public readonly record struct UVs(Vector2F Min, Vector2F Max);

[NativeMarshalling(typeof(SpriteMarshaller))]
public partial class Sprite : SceneObject
{
    public Texture? Texture
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(field, value))
                return;

            field = value;
            NativeSetTexture(this, value);
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
            if (field == value)
                return;

            field = value;
            NativeSetSize(this, value);
        }
    }

    public Color Tint
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetTint(this, value);
        }
    }

    public Vector2F Pivot
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetPivot(this, value);
        }
    }

    public UVs UVs
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetUVs(this, value);
        }
    }

    public SpriteDrawMode DrawMode
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetDrawMode(this, value);
        }
    }

    public Margin Margin
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetMargin(this, value);
        }
    }

    private Sprite(Scene scene, SceneObject? parent)
        : base(scene, parent, NativeCreate)
    {
        Size = new Vector2F(100, 100);
        Tint = new Color(1, 1, 1);
        UVs = new UVs(Vector2F.Zero, Vector2F.One);
    }

    public Sprite(Scene scene)
        : this(scene, null) { }

    public Sprite(SceneObject parent)
        : this(parent.Scene, parent) { }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_create")]
    private static partial IntPtr NativeCreate(Scene scene);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_texture")]
    private static partial void NativeSetTexture(Sprite id, Texture? texture);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_tint")]
    private static partial void NativeSetTint(Sprite id, Color color);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_pivot")]
    private static partial void NativeSetPivot(Sprite id, Vector2F pivot);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_size")]
    private static partial void NativeSetSize(Sprite id, Vector2F size);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_uv_rect")]
    private static partial void NativeSetUVs(Sprite id, UVs size);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_draw_mode")]
    private static partial void NativeSetDrawMode(Sprite id, SpriteDrawMode size);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_sprite_set_margin")]
    private static partial void NativeSetMargin(Sprite id, Margin margin);
}

[CustomMarshaller(typeof(Sprite), MarshalMode.ManagedToUnmanagedIn, typeof(SpriteMarshaller))]
public static class SpriteMarshaller
{
    public static IntPtr ConvertToUnmanaged(Sprite? sprite) => sprite?.NativeObject ?? IntPtr.Zero;
}
