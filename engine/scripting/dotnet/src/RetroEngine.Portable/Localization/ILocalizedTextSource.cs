// // @file LocalizedTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public enum LocalizedTextSourceCategory : byte
{
    Game,
    Engine,
    Editor,
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
    int Priority => 0;

    string? GetNativeCultureName(LocalizedTextSourceCategory category);

    IEnumerable<string> GetLocalizedCultureNames(LocalizedTextSourceCategory category);

    ILocalizedString? GetLocalizedString(TextId id, Locale locale, string fallback = "");
}
