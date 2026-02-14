using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        return param switch
        {
            null => null,
            IViewModel viewModel => viewModel.CreateView(),
            _ => new TextBlock { Text = "Not Found: " + param.GetType().Name },
        };
    }

    public bool Match(object? data)
    {
        return data is IViewModel;
    }
}
