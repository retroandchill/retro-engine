// // @file LocalizationResourceTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization;

public sealed partial class LocalizationResourceTextSource : ILocalizedTextSource
{
    private readonly List<int> _chunkIds = [];

    public string? GetNativeCultureName(LocalizedTextSourceCategory category)
    {
        throw new NotImplementedException();
    }

    public void GetLocalizedCultureNames(LocalizationLoadFlags flags, ISet<string> localizedCultureNames)
    {
        throw new NotImplementedException();
    }

    public void LoadLocalizedResources(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    )
    {
        throw new NotImplementedException();
    }

    public string? GetLocalizedString(TextId id, Culture cultureInfo, string fallback = "")
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public Task LoadLocalizedResourcesFromPathsAsync(
        ReadOnlyMemory<string> prioritizedNativePaths,
        ReadOnlyMemory<string> prioritizedLocalizedPaths,
        ReadOnlyMemory<string> gameNativePaths,
        LocalizationLoadFlags loadFlags,
        ReadOnlyMemory<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public void RegisterChunkId(int chunkId)
    {
        if (_chunkIds.Contains(chunkId))
            return;

        _chunkIds.Add(chunkId);
    }

    public void UnregisterChunkId(int chunkId)
    {
        _chunkIds.Remove(chunkId);
    }

    public bool HasRegisteredChunkId(int chunkId)
    {
        return _chunkIds.Contains(chunkId);
    }

    public IEnumerable<string> GetChunkedLocalizationTargets()
    {
        throw new NotImplementedException();
    }
}
