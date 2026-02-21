// // @file LocalizationResourceTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Localization;

public sealed class LocalizationResourceTextSource : ILocalizedTextSource
{
    public string? GetNativeCultureName(LocalizedTextSourceCategory category)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetLocalizedCultureNames(LocalizedTextSourceCategory category)
    {
        throw new NotImplementedException();
    }

    public void LoadLocalizedResources(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextId textId,
        TextLocalizationResource localizedResource
    )
    {
        throw new NotImplementedException();
    }

    public string? GetLocalizedString(TextId id, Culture cultureInfo, string fallback = "")
    {
        throw new NotImplementedException();
    }
}
