// @file TextBlock.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Portable.Localization;
using RetroEngine.Rendering.Text;

namespace RetroEngine.World;

[NativeMarshalling(typeof(TextBlockMarshaller))]
public sealed partial class TextBlock : SceneObject
{
    public Text Text
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            var textString = field.ToString();
            NativeSetText(this, textString, textString.Length);
        }
    }

    public FontFace? Font
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(field, value))
                return;

            field = value;
            NativeSetFont(this, field);
        }
    }

    public uint FontSize
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetFontSize(this, field);
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
            NativeSetTint(this, field);
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
            NativeSetPivot(this, field);
        }
    }

    public TextBlock(Scene scene)
        : this(scene, null) { }

    public TextBlock(SceneObject parent)
        : this(parent.Scene, parent) { }

    private TextBlock(Scene scene, SceneObject? parent)
        : base(scene, parent, NativeCreate)
    {
        FontSize = 48;
        Tint = new Color(1, 1, 1);
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_create")]
    private static partial IntPtr NativeCreate(Scene scene);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_set_text_utf16")]
    private static partial void NativeSetText(TextBlock id, ReadOnlySpan<char> text, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_set_font")]
    private static partial void NativeSetFont(TextBlock id, FontFace? font);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_set_font_size")]
    private static partial void NativeSetFontSize(TextBlock id, uint fontSize);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_set_tint")]
    private static partial void NativeSetTint(TextBlock id, Color color);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_text_block_set_pivot")]
    private static partial void NativeSetPivot(TextBlock id, Vector2F pivot);
}

[CustomMarshaller(typeof(TextBlock), MarshalMode.ManagedToUnmanagedIn, typeof(TextBlockMarshaller))]
public static class TextBlockMarshaller
{
    public static IntPtr ConvertToUnmanaged(TextBlock? sprite) => sprite?.NativeObject ?? IntPtr.Zero;
}
