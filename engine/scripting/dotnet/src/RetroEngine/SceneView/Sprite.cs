// // @file Sprite.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Assets;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;

namespace RetroEngine.SceneView;

public partial class Sprite : SceneObject
{
    public Sprite(SceneObject parent)
        : base(NativeCreate(parent.NativeObject))
    {
        Size = new Vector2F(100, 100);
        Tint = new Color(1, 1, 1);
    }

    public Texture? Texture
    {
        get;
        set
        {
            field = value;
            NativeSetTexture(NativeObject, value?.NativeObject ?? IntPtr.Zero);
        }
    }

    public Vector2F Size
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetSize(NativeObject, value);
        }
    }

    public Color Tint
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetTint(NativeObject, value);
        }
    }

    public Vector2F Pivot
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetPivot(NativeObject, value);
        }
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_create")]
    private static partial IntPtr NativeCreate(IntPtr id);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_texture")]
    private static partial void NativeSetTexture(IntPtr id, IntPtr texture);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_tint")]
    private static partial void NativeSetTint(IntPtr id, Color color);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_pivot")]
    private static partial void NativeSetPivot(IntPtr id, Vector2F pivot);

    [LibraryImport("retro_runtime", EntryPoint = "retro_sprite_set_size")]
    private static partial void NativeSetSize(IntPtr id, Vector2F size);
}
