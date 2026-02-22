// // @file LocalizedTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

[Flags]
public enum LocalizationLoadFlags : byte
{
    None = 0,
    Native = 1 << 0,
    Editor = 1 << 1,
    Game = 1 << 2,
    Engine = 1 << 3,
    Additional = 1 << 4,
    SkipExisting = 1 << 5,
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

    string? NativeCultureName { get; }

    void GetLocalizedCultureNames(LocalizationLoadFlags flags, ISet<string> localizedCultureNames);

    void LoadLocalizedResources(
        LocalizationLoadFlags loadFlags,
        IReadOnlyList<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    );

    ValueTask LoadLocalizedResourcesAsync(
        LocalizationLoadFlags loadFlags,
        IReadOnlyList<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource,
        CancellationToken cancellationToken = default
    );
}
