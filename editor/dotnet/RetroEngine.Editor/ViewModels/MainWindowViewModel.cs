using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels.Menus;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
public partial class MainWindowViewModel : ObservableObject
{
    public MenuBar Toolbar { get; init; } = new();
}
