// // @file ArchiveFormatterRegistry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using RetroEngine.Portable.Serialization.Binary.Formatters;
using RetroEngine.Portable.Serialization.Binary.Utilities;

namespace RetroEngine.Portable.Serialization.Binary;

public static class ArchiveFormatterRegistry
{
    private static readonly ConcurrentDictionary<Type, IArchiveFormatter> Formatters = new();

    static ArchiveFormatterRegistry()
    {
        WellKnownTypeRegistration.RegisterWellKnownTypesFormatters();
    }

    public static ArchiveFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.Formatter;
    }

    public static void Register<T>()
        where T : IArchivable
    {
        T.RegisterFormatters();
    }

    public static void Register<T>(ArchiveFormatter<T> formatter)
    {
        Formatters.TryAdd(typeof(T), formatter);
    }

    private static bool TryInvokeRegisterFormatter(Type type)
    {
        const string serializationNamespace = "RetroEngine.Portable.Serialization.Binary";
        const string archivableClass = $"{serializationNamespace}.{nameof(IArchivable)}";
        const string fullyQualifiedMethod = $"{archivableClass}.{nameof(IArchivable.RegisterFormatters)}";

        if (!typeof(IArchivable).IsAssignableFrom(type))
            return false;

        var method =
            type.GetMethod(fullyQualifiedMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? type.GetMethod(
                nameof(IArchivable.RegisterFormatters),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );
        if (method is null)
        {
            throw new InvalidOperationException(
                $"Type implements {nameof(IArchivable)} but can not found RegisterFormatter. Type: {type.FullName}"
            );
        }

        method.Invoke(null, null);
        return true;
    }

    // ReSharper disable once UnusedTypeParameter
    private static class Check<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static bool Registered { get; set; }
    }

    public static class Cache<T>
    {
        public static ArchiveFormatter<T> Formatter { get; set; } = null!;

        static Cache()
        {
            if (Check<T>.Registered)
                return;

            try
            {
                var type = typeof(T);
                if (TryInvokeRegisterFormatter(type))
                {
                    return;
                }

                if (TypeHelpers.IsAnonymous(type))
                {
                    Formatter = new ErrorArchiveFormatter<T>();
                }
                else
                {
                    var formatter = CreateGenericFormatter(type) as ArchiveFormatter<T>;
                    Formatter = formatter ?? new ErrorArchiveFormatter<T>();
                }
            }
            catch (Exception e)
            {
                Formatter = new ErrorArchiveFormatter<T>(e);
            }

            Formatters[typeof(T)] = Formatter;
            Check<T>.Registered = true;
        }
    }

    internal static object? CreateGenericFormatter(Type type)
    {
        Type? formatterType;
        if (type.IsArray)
        {
            if (type.IsSZArray)
            {
                formatterType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType()!);
            }
            else
            {
                formatterType = type.GetArrayRank() switch
                {
                    2 => typeof(TwoDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!),
                    3 => typeof(ThreeDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!),
                    4 => typeof(FourDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!),
                    _ => null,
                };
            }
        }
        else if (type.IsEnum)
        {
            formatterType = typeof(BlittableFormatter<>).MakeGenericType(type);
        }
        else
        {
            return null;
        }

        return formatterType is not null ? Activator.CreateInstance(formatterType) : null;
    }
}
