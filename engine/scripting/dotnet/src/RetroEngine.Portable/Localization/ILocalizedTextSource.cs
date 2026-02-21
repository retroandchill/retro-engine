// // @file LocalizedTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Localization;

public enum LocalizedTextSourceCategory : byte
{
    Game,
    Engine,
    Editor,
}

public enum QueryLocalizedResourceResult : byte
{
    Found,
    NotFound,
    NotImplemented,
}

[Flags]
public enum LocalizationLoadFlags : byte
{
    None = 0,
    Native = 1 << 0,
    Editor = 1 << 1,
    Game = 1 << 2,
    Engine = 1 << 3,
    Additional = 1 << 4,
    ForceLocalizedGame = 1 << 5,
    SkipExisting = 1 << 6,
}

public static class LocalizedTextSourcePriority
{
    public const int Lowest = -1000;
    public const int Low = -100;
    public const int Normal = 0;
    public const int High = 100;
    public const int Highest = 1000;
}

public interface ILocalizedTextSource
{
    int Priority => LocalizedTextSourcePriority.Normal;

    string? GetNativeCultureName(LocalizedTextSourceCategory category);

    IEnumerable<string> GetLocalizedCultureNames(LocalizedTextSourceCategory category);

    void LoadLocalizedResources(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextId textId,
        TextLocalizationResource localizedResource
    );

    QueryLocalizedResourceResult QueryLocalizedResource(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextId textId,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    )
    {
        return QueryLocalizedResourceResult.NotImplemented;
    }

    string? GetLocalizedString(TextId id, Culture cultureInfo, string fallback = "");
}

public static class LocalizedTextSourceExtensions
{
    extension(LocalizationLoadFlags loadFlags)
    {
        public bool ShouldLoadNative => loadFlags.HasFlag(LocalizationLoadFlags.Native);

        public bool ShouldLoadEditor => loadFlags.HasFlag(LocalizationLoadFlags.Editor);
        public bool ShouldLoadGame =>
            loadFlags.HasFlag(LocalizationLoadFlags.Game) | loadFlags.HasFlag(LocalizationLoadFlags.ForceLocalizedGame);

        public bool ShouldLoadEngine => loadFlags.HasFlag(LocalizationLoadFlags.Engine);

        public bool ShouldLoadAdditional => loadFlags.HasFlag(LocalizationLoadFlags.Additional);

        public bool ShouldLoadNativeGameData =>
            App.IsEditor && !loadFlags.HasFlag(LocalizationLoadFlags.ForceLocalizedGame);
    }
}
