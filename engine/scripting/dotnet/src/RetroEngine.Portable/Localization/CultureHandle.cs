// // @file CultureHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using ICU4N.Globalization;
using ICU4N.Text;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed class CultureHandle : IEquatable<CultureHandle>, IEqualityOperators<CultureHandle, CultureHandle, bool>
{
    public string Name => Culture.Name;
    internal CultureInfo Culture { get; }
    internal UCultureInfo IcuCulture { get; }

    internal string ShortDatePattern { get; }
    internal string MediumDatePattern { get; }
    internal string LongDatePattern { get; }
    internal string FullDatePattern { get; }
    internal string ShortTimePattern => Culture.DateTimeFormat.ShortTimePattern;
    internal string LongTimePattern => Culture.DateTimeFormat.LongTimePattern;

    private static readonly ConcurrentDictionary<
        (string CultureName, TextPluralType Type),
        PluralRules
    > PluralRulesCache = new();

    internal CultureHandle(string name)
    {
        Culture = CultureInfo.GetCultureInfo(name);
        IcuCulture = new UCultureInfo(name);
        var dateInfo = Culture.DateTimeFormat;
        var allShortDatePatterns = dateInfo.GetAllDateTimePatterns('d');
        var allLongDatePatterns = dateInfo.GetAllDateTimePatterns('D');

        ShortDatePattern = allShortDatePatterns.FirstOrDefault(d => !d.Contains("yyyy")) ?? dateInfo.ShortDatePattern;
        MediumDatePattern = allShortDatePatterns.FirstOrDefault(d => d.Contains("MMM")) ?? dateInfo.ShortDatePattern;
        LongDatePattern = allLongDatePatterns.FirstOrDefault(d => !d.Contains("dddd")) ?? dateInfo.LongDatePattern;
        FullDatePattern = allLongDatePatterns.FirstOrDefault(d => d.Contains("dddd")) ?? dateInfo.LongDatePattern;
    }

    public TextPluralForm GetPluralForm<T>(T value, TextPluralType pluralType)
        where T : unmanaged, INumber<T>
    {
        if (T.IsNaN(value) || T.IsInfinity(value))
            return TextPluralForm.Other;

        value = T.Abs(value);

        var number = double.CreateTruncating(value);
        if (double.IsNaN(number) || double.IsInfinity(number))
            return TextPluralForm.Other;

        var rules = PluralRulesCache.GetOrAdd(
            (Culture.Name, pluralType),
            static key =>
            {
                var (cultureName, type) = key;
                var locale = new UCultureInfo(cultureName);

                var pluralTypeIcu = type switch
                {
                    TextPluralType.Cardinal => PluralType.Cardinal,
                    TextPluralType.Ordinal => PluralType.Ordinal,
                    _ => PluralType.Cardinal,
                };

                return PluralRules.GetInstance(locale, pluralTypeIcu);
            }
        );

        var keyword = rules.Select(number);

        // ICU keywords: "zero", "one", "two", "few", "many", "other"
        return keyword switch
        {
            "zero" => TextPluralForm.Zero,
            "one" => TextPluralForm.One,
            "two" => TextPluralForm.Two,
            "few" => TextPluralForm.Few,
            "many" => TextPluralForm.Many,
            _ => TextPluralForm.Other,
        };
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
