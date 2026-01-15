using RetroEngine.Logging;

namespace RetroEngine.Async;

public abstract class AsyncGameSession : IGameSession, IDisposable
{
    private Task? _gameTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    public void Start()
    {
        if (_gameTask is not null)
            throw new InvalidOperationException("Game session is already running.");

        _gameTask = RunAsyncInternal(_cancellationTokenSource.Token);
    }

    private async Task RunAsyncInternal(CancellationToken cancellationToken)
    {
        try
        {
            var exitCode = await RunAsync(cancellationToken);
            Engine.RequestShutdown(exitCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Engine.RequestShutdown();
        }
        catch (Exception e)
        {
            Logger.Critical(e.ToString());
            Engine.RequestShutdown(1);
        }
    }

    protected abstract Task<int> RunAsync(CancellationToken cancellationToken);

    public void Stop()
    {
        if (_gameTask is null)
            throw new InvalidOperationException("Game session was not running.");

        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }
}
