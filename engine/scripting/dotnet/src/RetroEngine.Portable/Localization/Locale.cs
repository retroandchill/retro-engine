// // @file Locale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Numerics;

namespace RetroEngine.Portable.Localization;

public class Locale : IEquatable<Locale>, IEqualityOperators<Locale, Locale, bool>
{
    private readonly CultureInfo _cultureInfo;
    public string LocaleTag { get; }
    public string DisplayName { get; }
    public string Name { get; }
    public string EnglishName { get; }
    public string Language { get; }
    public string Region { get; }
    public bool IsRightToLeft { get; }

    public Locale(string name)
    {
        try
        {
            _cultureInfo = CultureInfo.GetCultureInfo(name);
            LocaleTag = name;
        }
        catch (CultureNotFoundException)
        {
            _cultureInfo = CultureInfo.GetCultureInfo("en-US");
            LocaleTag = "en-US";
        }

        DisplayName = _cultureInfo.DisplayName;
        Name = _cultureInfo.Name;
        EnglishName = _cultureInfo.EnglishName;
        Language = _cultureInfo.TwoLetterISOLanguageName;
        Region = _cultureInfo.Name.Contains('-') ? _cultureInfo.Name[(_cultureInfo.Name.LastIndexOf('-') + 1)..] : "";
        IsRightToLeft = _cultureInfo.TextInfo.IsRightToLeft;
    }

    public override bool Equals(object? obj)
    {
        return obj is Locale other && Equals(other);
    }

    public bool Equals(Locale? other)
    {
        return this == other;
    }

    public static bool operator ==(Locale? left, Locale? right)
    {
        if (left is null)
            return right is null;

        return right is not null && left._cultureInfo.Equals(right._cultureInfo);
    }

    public static bool operator !=(Locale? left, Locale? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return _cultureInfo.GetHashCode();
    }
}
