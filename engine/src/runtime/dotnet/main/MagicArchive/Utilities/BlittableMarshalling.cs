// // @file BlittableMarshalling.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MagicArchive.Formatters;

// ReSharper disable StaticMemberInGenericType

namespace MagicArchive.Utilities;

public enum BlittableType
{
    NotBlittable,
    BlittableSimple,
    BlittableComplex,
}

public static class BlittableMarshalling
{
    private const int NonBlittableValue = -1;

    private readonly record struct BlittableTypeInfo(BlittableType Type, int Size, int Alignment)
    {
        public static readonly BlittableTypeInfo NotBlittable = new(
            BlittableType.NotBlittable,
            NonBlittableValue,
            NonBlittableValue
        );
    }

    private static readonly ConcurrentDictionary<Type, BlittableTypeInfo> BlittableCache = new();

    static BlittableMarshalling()
    {
        RegisterSimpleBlittable<byte>();
        RegisterSimpleBlittable<sbyte>();
        RegisterSimpleBlittable<short>();
        RegisterSimpleBlittable<ushort>();
        RegisterSimpleBlittable<int>();
        RegisterSimpleBlittable<uint>();
        RegisterSimpleBlittable<long>();
        RegisterSimpleBlittable<ulong>();
        RegisterSimpleBlittable<float>();
        RegisterSimpleBlittable<double>();
        RegisterSimpleBlittable<char>();
        RegisterSimpleBlittable<Half>();
        RegisterSimpleBlittable<Int128>();
        RegisterSimpleBlittable<UInt128>();
        RegisterSimpleBlittable<Rune>();
        RegisterSimpleBlittable<DateTime>();

        RegisterNotBlittable<bool>();
        RegisterNotBlittable<decimal>();
        RegisterNotBlittable<IntPtr>();
        RegisterNotBlittable<UIntPtr>();
        RegisterNotBlittable<DateTimeOffset>();
        RegisterNotBlittable<Guid>();
    }

    public static bool IsRegistered<T>() => Check<T>.IsRegistered;

    public static bool IsBlittable<T>() => Cache<T>.Type != BlittableType.NotBlittable;

    public static bool IsBlittable(Type type)
    {
        return GetBlittableTypeInfo(type).Type != BlittableType.NotBlittable;
    }

    private static BlittableTypeInfo GetBlittableTypeInfo(Type type)
    {
        if (BlittableCache.TryGetValue(type, out var blittableType))
        {
            return blittableType;
        }

        var blittableState = CheckIfTypeIsBlittable(type);
        BlittableCache[type] = blittableState;

        return blittableState;
    }

    public static bool IsSimpleBlittable<T>() => Cache<T>.Type == BlittableType.BlittableSimple;

    public static bool IsSimpleBlittable(Type type) => GetBlittableTypeInfo(type).Type == BlittableType.BlittableSimple;

    private struct AlignmentHelper<T>
    {
        public byte Padding;
        public T Target;
    }

    private static void RegisterBlittable<T>(BlittableType type)
    {
        Check<T>.IsRegistered = true;
        if (type == BlittableType.NotBlittable)
        {
            BlittableCache[typeof(T)] = BlittableTypeInfo.NotBlittable;
        }
        else
        {
            AlignmentHelper<T> alignmentHelper = default;
            BlittableCache[typeof(T)] = new BlittableTypeInfo(
                type,
                Unsafe.SizeOf<T>(),
                (int)Unsafe.ByteOffset(ref alignmentHelper.Padding, ref Unsafe.As<T, byte>(ref alignmentHelper.Target!))
            );
        }
        Cache<T>.Type = type;
    }

    public static void RegisterNotBlittable<T>()
    {
        RegisterBlittable<T>(BlittableType.NotBlittable);
    }

    public static void RegisterSimpleBlittable<T>()
        where T : unmanaged
    {
        RegisterBlittable<T>(BlittableType.BlittableSimple);
    }

    public static void RegisterComplexBlittable<T>()
        where T : unmanaged
    {
        RegisterBlittable<T>(BlittableType.BlittableComplex);
    }

    // ReSharper disable once UnusedTypeParameter
    private static class Check<T>
    {
        public static bool IsRegistered { get; set; }
    }

    private static class Cache<T>
    {
        public static BlittableType Type { get; set; }
        public static int Size { get; set; }
        public static int Alignment { get; set; }

        static Cache()
        {
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            if (Check<T>.IsRegistered)
                return;

            try
            {
                (Type, Size, Alignment) = CheckIfTypeIsBlittable<T>();
            }
            catch
            {
                Type = BlittableType.NotBlittable;
                Size = NonBlittableValue;
                Alignment = NonBlittableValue;
            }

            try
            {
                BlittableCache[typeof(T)] = new BlittableTypeInfo(Type, Size, Alignment);
            }
            catch
            {
                // Do nothing, failing to add the cache entry is not a problem.
            }

            Check<T>.IsRegistered = true;
        }
    }

    private static BlittableTypeInfo CheckIfTypeIsBlittable<T>()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return BlittableTypeInfo.NotBlittable;
        }

        if (typeof(T).IsEnum)
            return new BlittableTypeInfo(BlittableType.BlittableSimple, Unsafe.SizeOf<T>(), Unsafe.SizeOf<T>());

        var layoutAttribute = typeof(T).StructLayoutAttribute;
        if (layoutAttribute is not null)
        {
            if (
                layoutAttribute.Value != LayoutKind.Sequential
                || (layoutAttribute.Size != Unsafe.SizeOf<T>() && layoutAttribute.Size != 0)
                || layoutAttribute.Pack != 0
            )
                return BlittableTypeInfo.NotBlittable;
        }

        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (
            fields.Length == 1
            && BlittableCache.TryGetValue(fields[0].FieldType, out var singleField)
            && singleField.Type == BlittableType.BlittableSimple
        )
            return singleField;

        if (!typeof(T).IsGenericType)
            return BlittableTypeInfo.NotBlittable;

        var definition = typeof(T).GetGenericTypeDefinition();
        if (!TupleFormatters.ValueTupleTypes.Contains(definition))
            return BlittableTypeInfo.NotBlittable;

        var blittableType = BlittableType.BlittableSimple;
        var maxAlignment = 0;
        var runningSize = 0;
        var i = 0;
        foreach (var field in fields)
        {
            if (!field.FieldType.IsValueType)
                return BlittableTypeInfo.NotBlittable;

            if (!BlittableCache.TryGetValue(field.FieldType, out var fieldInfo))
            {
                fieldInfo = CheckIfTypeIsBlittable(field.FieldType);
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (fieldInfo.Type)
            {
                case BlittableType.NotBlittable:
                    return BlittableTypeInfo.NotBlittable;
                case BlittableType.BlittableComplex:
                    blittableType = BlittableType.BlittableComplex;
                    break;
            }

            if (i > 0)
                blittableType = BlittableType.BlittableComplex;

            if (fieldInfo.Alignment > maxAlignment)
            {
                maxAlignment = fieldInfo.Alignment;
            }

            // If the field is not aligned, it's not blittable.
            if (runningSize % fieldInfo.Alignment != 0)
                return BlittableTypeInfo.NotBlittable;

            runningSize += fieldInfo.Size;
            i++;
        }

        return Unsafe.SizeOf<T>() - runningSize == 0
            ? new BlittableTypeInfo(blittableType, Unsafe.SizeOf<T>(), maxAlignment)
            : BlittableTypeInfo.NotBlittable;
    }

    private static BlittableTypeInfo CheckIfTypeIsBlittable(Type type)
    {
        var cache = typeof(Cache<>).MakeGenericType(type);
        var typeProperty = cache.GetProperty(nameof(Cache<>.Type), BindingFlags.Public | BindingFlags.Static);
        var sizeProperty = cache.GetProperty(nameof(Cache<>.Size), BindingFlags.Public | BindingFlags.Static);
        var alignmentProperty = cache.GetProperty(nameof(Cache<>.Alignment), BindingFlags.Public | BindingFlags.Static);
        if (typeProperty is null || sizeProperty is null || alignmentProperty is null)
            return BlittableTypeInfo.NotBlittable;
        return new BlittableTypeInfo(
            (BlittableType)typeProperty.GetValue(null)!,
            (int)sizeProperty.GetValue(null)!,
            (int)alignmentProperty.GetValue(null)!
        );
    }

    public static T ReverseEndianness<T>(T value)
    {
        ReverseEndianness(ref value);
        return value;
    }

    public static void ReverseEndianness<T>(ref T value)
    {
        switch (Cache<T>.Type)
        {
            case BlittableType.BlittableSimple:
                {
                    var bytes = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());
                    bytes.Reverse();
                }
                break;
            case BlittableType.BlittableComplex:
                throw new NotSupportedException("Complex blittable types are not supported.");
            case BlittableType.NotBlittable:
            default:
                throw new NotSupportedException($"Type {typeof(T).Name} is not blittable.");
        }
    }
}
