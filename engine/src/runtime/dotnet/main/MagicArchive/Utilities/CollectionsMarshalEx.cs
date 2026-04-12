// // @file CollectionsMarshalEx.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable CS8618
#pragma warning disable CS0649

using System.Runtime.CompilerServices;

namespace MagicArchive.Utilities;

internal static class CollectionsMarshalEx
{
    /// <summary>
    /// similar as AsSpan but modify size to create fixed-size span.
    /// </summary>
    public static Span<T?> CreateSpan<T>(List<T?> list, int length)
    {
        list.EnsureCapacity(length);

        ref var view = ref Unsafe.As<List<T?>, ListView<T?>>(ref list);
        view._size = length;
        return view._items.AsSpan(0, length);
    }

    public static Span<T?> AsSpan<T>(Stack<T?> stack)
    {
        ref var view = ref Unsafe.As<Stack<T?>, StackView<T?>>(ref stack);
        return view._items.AsSpan(0, view._size);
    }

    public static Span<T?> CreateSpan<T>(Stack<T?> stack, int length)
    {
        stack.EnsureCapacity(length);

        ref var view = ref Unsafe.As<Stack<T?>, StackView<T?>>(ref stack);
        view._size = length;
        return view._items.AsSpan(0, view._size);
    }

    internal sealed class ListView<T>
    {
        public T[] _items;
        public int _size;
        public int _version;
    }

    internal sealed class StackView<T>
    {
        public T[] _items;
        public int _size;
        public int _version;
    }
}
