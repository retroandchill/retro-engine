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
using Rendering_Texture = RetroEngine.Rendering.Texture;

namespace RetroEngine.World;

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
            field = value;
            NativeSetUVs(this, value);
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_create")]
    private static partial IntPtr NativeCreate(Scene scene, SceneObject? id);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_set_texture")]
    private static partial void NativeSetTexture(Sprite id, Rendering_Texture? texture);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_set_tint")]
    private static partial void NativeSetTint(Sprite id, Color color);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_set_pivot")]
    private static partial void NativeSetPivot(Sprite id, Vector2F pivot);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_set_size")]
    private static partial void NativeSetSize(Sprite id, Vector2F size);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_sprite_set_uv_rect")]
    private static partial void NativeSetUVs(Sprite id, UVs size);
}

[CustomMarshaller(typeof(Sprite), MarshalMode.ManagedToUnmanagedIn, typeof(SpriteMarshaller))]
public static class SpriteMarshaller
{
    public static IntPtr ConvertToUnmanaged(Sprite? sprite) => sprite?.NativeObject ?? IntPtr.Zero;
}
