using System;
using Avalonia.Controls;
using RetroEngine.Editor.ViewModels;
using Serilog;

namespace RetroEngine.Editor.Views;

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
