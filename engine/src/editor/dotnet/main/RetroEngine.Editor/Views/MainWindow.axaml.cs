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
        Engine?.PollPlatformEvents();
        RequestAnimationFrame(Tick);
    }
}
