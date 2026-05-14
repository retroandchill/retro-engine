// // @file EditableAssetSessionManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Injectio.Attributes;
using RetroEngine.Assets;

namespace RetroEngine.AssetTools.Editing;

[RegisterSingleton]
public sealed class EditableAssetSessionManager : IAsyncDisposable
{
    private readonly Dictionary<AssetPath, IEditableAssetSession> _sessions = [];

    public IReadOnlyCollection<IEditableAssetSession> Sessions => _sessions.Values;

    public void Add(IEditableAssetSession session)
    {
        _sessions.Add(session.Path, session);
    }

    public bool Remove(AssetPath path, [NotNullWhen(true)] out IEditableAssetSession? session)
    {
        return _sessions.Remove(path, out session);
    }

    public async ValueTask SaveAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var session in _sessions.Values.ToArray())
        {
            await session.SaveAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var session in _sessions.Values.ToArray())
        {
            await session.DisposeAsync();
        }

        _sessions.Clear();
    }
}
