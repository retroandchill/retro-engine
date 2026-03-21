using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
public sealed partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
{
    public object? Content
    {
        get;
        set
        {
            if (field is IDisposable disposable)
            {
                disposable.Dispose();
            }

            SetProperty(ref field, value);
        }
    }
}
