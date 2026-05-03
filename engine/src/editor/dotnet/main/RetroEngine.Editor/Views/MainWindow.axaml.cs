using System;
using Avalonia.Controls;
using RetroEngine.Editor.ViewModels;
using Serilog;

namespace RetroEngine.Editor.Views;

public partial class MainWindow : Window
{
    public Engine? Engine { get; init; }

    public MainWindow()
    {
        InitializeComponent();
        RequestAnimationFrame(Tick);
    }

    private void Tick(TimeSpan time)
    {
        try
        {
            Engine?.PollPlatformEvents();
        }
        catch (ObjectDisposedException)
        {
            // This is an edge case where it tries to tick after the engine has been disposed, this should protect against that
        }

        RequestAnimationFrame(Tick);
    }
}
