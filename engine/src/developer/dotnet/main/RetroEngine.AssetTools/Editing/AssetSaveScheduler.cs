// // @file AssetSaveScheduler.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Serilog;

namespace RetroEngine.AssetTools.Editing;

public sealed class AssetSaveScheduler : IAsyncDisposable
{
    private readonly TimeSpan _debounceDelay;
    private readonly TimeSpan _maxDelay;
    private readonly Func<CancellationToken, ValueTask> _saveFunc;
    private readonly Lock _lock = new();

    private CancellationTokenSource? _debounceCancellationTokenSource;
    private CancellationTokenSource? _maximumSaveCancellationTokenSource;
    private bool _dirty;
    private bool _disposed;
    private bool _saveInProgress;
    private bool _saveRequestedDuringSave;

    public bool IsDirty
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _dirty;
        }
    }

    public bool IsSaving
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _saveInProgress;
        }
    }

    public AssetSaveScheduler(TimeSpan debounceDelay, TimeSpan maxDelay, Func<CancellationToken, ValueTask> saveFunc)
    {
        if (debounceDelay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(debounceDelay),
                debounceDelay,
                "Debounce delay must be greater than zero."
            );

        if (maxDelay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxDelay), maxDelay, "Max delay must be greater than zero.");

        if (maxDelay < debounceDelay)
            throw new ArgumentException("Max delay must be greater than or equal to debounce delay.");

        _debounceDelay = debounceDelay;
        _maxDelay = maxDelay;
        _saveFunc = saveFunc;
    }

    public void MarkDirty()
    {
        ThrowIfDisposed();

        using var scope = _lock.EnterScope();
        _dirty = true;
        RestartDebounceTimer();
        _maximumSaveCancellationTokenSource ??= StartMaximumSaveTimer();
    }

    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancelTimers();

        await SaveIfDirtyAsync(cancellationToken);
    }

    private void RestartDebounceTimer()
    {
        if (_debounceCancellationTokenSource is not null)
        {
            _debounceCancellationTokenSource.Cancel();
            _debounceCancellationTokenSource.Dispose();
        }

        _debounceCancellationTokenSource = new CancellationTokenSource();
        _ = RunDelayedSaveAsync(_debounceDelay, _debounceCancellationTokenSource.Token);
    }

    private CancellationTokenSource StartMaximumSaveTimer()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        _ = RunDelayedSaveAsync(_maxDelay, cancellationTokenSource.Token);

        return cancellationTokenSource;
    }

    private async Task RunDelayedSaveAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            await SaveIfDirtyAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Swallow exception if cancellation was requested.
        }
    }

    private async ValueTask SaveIfDirtyAsync(CancellationToken cancellationToken)
    {
        using (_lock.EnterScope())
        {
            if (!_dirty)
                return;

            if (_saveInProgress)
            {
                _saveRequestedDuringSave = true;
                return;
            }

            _dirty = false;
            _saveInProgress = true;

            CancelTimersCore();
        }

        try
        {
            await _saveFunc(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // We don't want to log for this one
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save asset.");
        }
        finally
        {
            bool shouldSaveAgain;
            using (_lock.EnterScope())
            {
                _saveInProgress = false;
                shouldSaveAgain = _dirty || _saveRequestedDuringSave;
                _saveRequestedDuringSave = false;
            }

            if (shouldSaveAgain)
            {
                await SaveIfDirtyAsync(cancellationToken);
            }
        }
    }

    private void CancelTimers()
    {
        using var scope = _lock.EnterScope();
        CancelTimersCore();
    }

    private void CancelTimersCore()
    {
        if (_debounceCancellationTokenSource is not null)
        {
            _debounceCancellationTokenSource.Cancel();
            _debounceCancellationTokenSource.Dispose();
            _debounceCancellationTokenSource = null;
        }

        if (_maximumSaveCancellationTokenSource is null)
            return;

        _maximumSaveCancellationTokenSource.Cancel();
        _maximumSaveCancellationTokenSource.Dispose();
        _maximumSaveCancellationTokenSource = null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        CancelTimers();

        await SaveIfDirtyAsync(CancellationToken.None);
        _disposed = true;
    }
}
