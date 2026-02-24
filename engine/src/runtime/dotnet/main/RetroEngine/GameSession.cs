namespace RetroEngine;

public interface IGameSession
{
    void Start();

    void Stop();
}

public static class GameSessionExtensions
{
    public static void Terminate(this IGameSession session)
    {
        session.Stop();
        if (session is IDisposable disposable)
            disposable.Dispose();
    }
}
