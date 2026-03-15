using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
public partial class MainWindowViewModel : ObservableObject
{
    public IViewModel? Content
    {
        get;
        set => SetProperty(ref field, value);
    }
}
