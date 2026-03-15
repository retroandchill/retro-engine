using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RegisterSingleton]
public class ViewLocator : IViewLocator, IDataTemplate
{
    public ViewDefinition Locate(object viewModel)
    {
        return viewModel is IViewModel vm
            ? new ViewDefinition(vm.ViewType, () => vm.CreateView())
            : throw new TypeLoadException("View not found.");
    }

    public object Create(object viewModel)
    {
        return Build(viewModel) ?? throw new TypeLoadException("View not found.");
    }

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
