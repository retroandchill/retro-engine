using Avalonia.Controls;
using RetroEngine.Editor.ViewModels;

namespace RetroEngine.Editor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
