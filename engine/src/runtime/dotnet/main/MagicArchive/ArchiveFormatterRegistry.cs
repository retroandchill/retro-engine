// // @file ArchiveFormatterRegistry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using MagicArchive.Formatters;
using MagicArchive.Utilities;

namespace MagicArchive;

public static class ArchiveFormatterRegistry
{
    private static readonly ConcurrentDictionary<Type, IArchiveFormatter> Formatters = new();

    static ArchiveFormatterRegistry()
    {
        WellKnownTypeRegistration.RegisterWellKnownTypesFormatters();
    }

    public static void Register<T>()
        where T : IArchivable
    {
        T.RegisterFormatters();
    }

    public static void Register<T>(ArchiveFormatter<T> formatter)
    {
        Check<T>.Registered = true;
        Formatters[typeof(T)] = formatter;
        Cache<T>.Formatter = formatter;
    }

    public static bool IsRegistered<T>() => Check<T>.Registered;

    public static IArchiveFormatter GetFormatter(Type type)
    {
        if (Formatters.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        if (TryInvokeRegisterFormatter(type))
        {
            if (Formatters.TryGetValue(type, out formatter))
            {
                return formatter;
            }
        }

        if (type.IsAnonymous)
        {
            formatter = new ErrorArchiveFormatter(
                type,
                "Serialize anonymous type is not supported, use record or tuple instead."
            );
        }
        else
        {
            if (CreateGenericFormatter(type) is IArchiveFormatter candidate)
            {
                formatter = candidate;
            }
            else
            {
                formatter = new ErrorArchiveFormatter(type);
            }
        }

        Formatters[type] = formatter;
        return formatter;
    }

    public static ArchiveFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.Formatter;
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

                if (type.IsAnonymous)
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
            formatterType = typeof(EnumFormatter<>).MakeGenericType(type);
        }
        else
        {
            ReadOnlySpan<ImmutableDictionary<Type, Type>> possibleFormatters =
            [
                TupleFormatters.FormatterTypes,
                CollectionFormatters.FormatterTypes,
            ];

            formatterType = null;
            foreach (var formatter in possibleFormatters)
            {
                formatterType = TryCreateGenericFormatterType(type, formatter);
                if (formatterType is not null)
                    break;
            }
        }

        return formatterType is not null ? Activator.CreateInstance(formatterType) : null;
    }

    private static Type? TryCreateGenericFormatterType(Type type, ImmutableDictionary<Type, Type> formatters)
    {
        if (!type.IsGenericType)
            return null;
        var genericDefinition = type.GetGenericTypeDefinition();

        return formatters.GetValueOrDefault(genericDefinition)?.MakeGenericType(type.GetGenericArguments());
    }
}
