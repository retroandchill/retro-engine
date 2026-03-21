using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
public partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
{
    [ObservableProperty]
    public partial IViewModel? Content { get; set; }
}
