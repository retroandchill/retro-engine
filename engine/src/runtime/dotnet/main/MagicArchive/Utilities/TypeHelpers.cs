// // @file TypeHelpers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable StaticMemberInGenericType

namespace MagicArchive.Utilities;

internal static class TypeHelpers
{
    private static readonly MethodInfo IsBlittableMethod = typeof(BinaryHandling).GetMethod(
        nameof(BinaryHandling.IsBlittable),
        BindingFlags.Static | BindingFlags.Public
    )!;
    private static readonly MethodInfo UnsafeSizeOf = typeof(Unsafe).GetMethod(
        nameof(Unsafe.SizeOf),
        BindingFlags.Static | BindingFlags.Public
    )!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReferenceOrNullable<T>()
    {
        return Cache<T>.IsReferenceOrNullable;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeKind TryGetArchivableSize<T>(out int size)
    {
        if (Cache<T>.IsBlittableSZArray)
        {
            size = Cache<T>.BlittableSZArrayElementSize;
            return TypeKind.BlittableSZArray;
        }

        if (Cache<T>.IsFixedSizeArchivable)
        {
            size = Cache<T>.ArchivableFixedSize;
            return TypeKind.FixedSizeArchivable;
        }

        size = 0;
        return TypeKind.None;
    }

    extension(Type type)
    {
        public bool IsAnonymous
        {
            [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                type.Namespace == null
                && type.IsSealed
                && (
                    type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal)
                    || type.Name.StartsWith("<>__AnonType", StringComparison.Ordinal)
                    || type.Name.StartsWith("VB$AnonymousType_", StringComparison.Ordinal)
                )
                && type.IsDefined(typeof(CompilerGeneratedAttribute), false);
        }
    }

    private static class Cache<T>
    {
        public static bool IsReferenceOrNullable { get; set; }
        public static bool IsBlittableSZArray { get; set; }
        public static int BlittableSZArrayElementSize { get; set; }
        public static bool IsFixedSizeArchivable { get; set; }
        public static int ArchivableFixedSize { get; set; }

        static Cache()
        {
            try
            {
                var type = typeof(T);
                IsReferenceOrNullable = !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

                if (type.IsSZArray)
                {
                    var elementType = type.GetElementType();
                    var isBlittable = (bool)IsBlittableMethod.MakeGenericMethod(elementType!).Invoke(null, null)!;
                    if (!isBlittable)
                        return;
                    IsBlittableSZArray = true;
                    BlittableSZArrayElementSize = (int)UnsafeSizeOf.MakeGenericMethod(elementType!).Invoke(null, null)!;
                }
                else
                {
                    const string archivableNamespace = "RetroEngine.Portable.Serialization.Binary";
                    const string archivableClass = $"{archivableNamespace}.{nameof(IFixedSizeArchivable)}";
                    const string archivableProperty = $"{archivableClass}.{nameof(IFixedSizeArchivable.Size)}";
                    if (!typeof(IFixedSizeArchivable).IsAssignableFrom(type))
                        return;
                    var property = type.GetProperty(
                        archivableProperty,
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic
                    );
                    if (property is null)
                        return;
                    IsFixedSizeArchivable = true;
                    ArchivableFixedSize = (int)property.GetValue(null)!;
                }
            }
            catch
            {
                IsBlittableSZArray = false;
                IsFixedSizeArchivable = false;
            }
        }
    }

    public enum TypeKind : byte
    {
        None,
        BlittableSZArray,
        FixedSizeArchivable,
    }
}
