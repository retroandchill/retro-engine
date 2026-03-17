using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class MainWindowViewModel(IServiceProvider serviceProvider) : ObservableObject, IMainWindowViewModel
{
    public IViewModel? Content
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void ShowProjectOpen()
    {
        var launchScreen = serviceProvider.GetRequiredService<LaunchScreenViewModel>();
        _ = launchScreen.OnDisplayedAsync();
        Content = launchScreen;
    }

    public void ShowMainEditor()
    {
        throw new NotImplementedException();
    }
}
