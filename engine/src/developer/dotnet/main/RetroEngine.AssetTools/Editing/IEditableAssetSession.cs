// @file IEditableAssetSession.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;

namespace RetroEngine.AssetTools.Editing;

public interface IEditableAssetSession : IAsyncDisposable
{
    AssetPath Path { get; }

    object Asset { get; }

    bool IsDirty { get; }

    bool IsSaving { get; }

    ValueTask SaveAsync(CancellationToken cancellationToken = default);

    ValueTask FlushAsync(CancellationToken cancellationToken = default);
}
