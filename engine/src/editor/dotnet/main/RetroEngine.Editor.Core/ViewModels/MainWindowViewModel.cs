using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views;

namespace RetroEngine.Editor.Core.ViewModels;

[ViewModelFor<MainWindow>]
[RegisterSingleton(Registration = RegistrationStrategy.Self)]
public partial class MainWindowViewModel(IServiceProvider serviceProvider) : ObservableObject
{
    public IViewModel? Content
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public void ShowProjectOpen()
    {
        Content = serviceProvider.GetRequiredService<LaunchScreenViewModel>();
    }

    public void ShowMainEditor()
    {
        throw new NotImplementedException();
    }
}
