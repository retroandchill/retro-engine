// // @file CultureHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Numerics;

namespace RetroEngine.Portable.Localization;

public sealed class CultureHandle : IEquatable<CultureHandle>, IEqualityOperators<CultureHandle, CultureHandle, bool>
{
    public string Name => Culture.Name;
    internal CultureInfo Culture { get; }

    internal string ShortDatePattern { get; }
    internal string MediumDatePattern { get; }
    internal string LongDatePattern { get; }
    internal string FullDatePattern { get; }
    internal string ShortTimePattern => Culture.DateTimeFormat.ShortTimePattern;
    internal string LongTimePattern => Culture.DateTimeFormat.LongTimePattern;

    internal CultureHandle(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;
        var dateInfo = cultureInfo.DateTimeFormat;
        var allShortDatePatterns = dateInfo.GetAllDateTimePatterns('d');
        var allLongDatePatterns = dateInfo.GetAllDateTimePatterns('D');

        ShortDatePattern = allShortDatePatterns.FirstOrDefault(d => !d.Contains("yyyy")) ?? dateInfo.ShortDatePattern;
        MediumDatePattern = allShortDatePatterns.FirstOrDefault(d => d.Contains("MMM")) ?? dateInfo.ShortDatePattern;
        LongDatePattern = allLongDatePatterns.FirstOrDefault(d => !d.Contains("dddd")) ?? dateInfo.LongDatePattern;
        FullDatePattern = allLongDatePatterns.FirstOrDefault(d => d.Contains("dddd")) ?? dateInfo.LongDatePattern;
    }

    public override bool Equals(object? obj)
    {
        return obj is CultureHandle other && Equals(other);
    }

    public bool Equals(CultureHandle? other)
    {
        return other is not null && Name == other.Name;
    }

    public static bool operator ==(CultureHandle? left, CultureHandle? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(CultureHandle? left, CultureHandle? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
