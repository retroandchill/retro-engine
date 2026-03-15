using Avalonia.Controls;

namespace RetroEngine.Editor.Core.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RequestAnimationFrame(Tick);
    }

    private void Tick(TimeSpan time)
    {
        if (Engine.IsInitialized)
        {
            Engine.Instance.PollPlatformEvents();
        }

        RequestAnimationFrame(Tick);
    }
}
