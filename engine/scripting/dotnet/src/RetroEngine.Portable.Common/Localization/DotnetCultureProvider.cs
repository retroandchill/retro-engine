// // @file DotnetCultureProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace RetroEngine.Portable.Localization;

public sealed class DotnetCultureProvider : ICultureProvider
{
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;
    public event Action<CultureInfo>? CurrentCultureChanged;

    public bool TrySetCurrentCulture(CultureInfo culture)
    {
        CurrentCulture = culture;
        CurrentCultureChanged?.Invoke(culture);
        return true;
    }

    public string CurrentNativeCultureName => CultureInfo.CurrentCulture.Name;

    public bool TrySetNativeCultureName(string cultureName)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            return TrySetCurrentCulture(culture);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    public IEnumerable<CultureInfo> GetAvailableCultures()
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures);
    }

    public IEnumerable<string> GetAvailableNativeCultureNames()
    {
        return GetAvailableCultures().Select(c => c.Name);
    }
}
