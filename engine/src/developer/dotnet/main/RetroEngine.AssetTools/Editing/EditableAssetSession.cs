// // @file EditableAssetSession.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.AssetTools.ViewModels;

namespace RetroEngine.AssetTools.Editing;

public sealed class EditableAssetSession : IEditableAssetSession
{
    private readonly IAssetEditorViewModel _editor;
    private readonly Func<object, CancellationToken, ValueTask> _saveFunc;
    private readonly AssetSaveScheduler _saveScheduler;

    public AssetPath Path => _editor.Path;
    public object Asset => _editor.Asset;

    public bool IsDirty => _saveScheduler.IsDirty;

    public bool IsSaving => _saveScheduler.IsSaving;

    public EditableAssetSession(
        IAssetEditorViewModel editor,
        Func<object, CancellationToken, ValueTask> saveFunc,
        TimeSpan debounceDelay,
        TimeSpan maxDelay
    )
    {
        _editor = editor;
        _saveFunc = saveFunc;

        _saveScheduler = new AssetSaveScheduler(debounceDelay, maxDelay, SaveInternalAsync);
        _editor.AssetChanged += OnAssetChanged;
    }

    private void OnAssetChanged()
    {
        _saveScheduler.MarkDirty();
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await _saveScheduler.FlushAsync(cancellationToken);
    }

    private async ValueTask SaveInternalAsync(CancellationToken cancellationToken)
    {
        await _saveFunc(Asset, cancellationToken);
    }

    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        await _saveScheduler.FlushAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        _editor.AssetChanged -= OnAssetChanged;

        await _saveScheduler.DisposeAsync();
    }
}
