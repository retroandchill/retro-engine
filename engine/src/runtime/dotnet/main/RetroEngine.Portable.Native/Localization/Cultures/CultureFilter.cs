// // @file CultureFilter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

public sealed class CultureFilter
{
    private readonly HashSet<string> _enabledCultures = [];
    private readonly HashSet<string> _disabledCultures = [];

    public CultureFilter(HashSet<string>? availableCulture = null)
    {
        Init(LocalizationLoadFlags.Default, availableCulture);
    }

    public CultureFilter(LocalizationLoadFlags targetFlags, HashSet<string>? availableCulture = null)
    {
        Init(targetFlags, availableCulture);
    }

    private void Init(LocalizationLoadFlags targetFlags, HashSet<string>? availableCulture)
    {
        // TODO: For now we don't have config loading so this doesn't matter
    }

    public bool IsCultureAllowed(string cultureName)
    {
        return (_enabledCultures.Count == 0 || _enabledCultures.Contains(cultureName))
            && !_disabledCultures.Contains(cultureName);
    }
}
