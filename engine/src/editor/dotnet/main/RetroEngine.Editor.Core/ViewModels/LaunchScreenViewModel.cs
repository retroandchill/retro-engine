// // @file ProjectOpenWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Editor.Core.Views;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels;

public interface ILaunchScreenTabViewModel : IViewModel
{
    Text Header { get; }

    Task OnDisplayedAsync(CancellationToken cancellationToken);
}

[ViewModelFor<LaunchScreenView>]
public sealed partial class LaunchScreenViewModel : ObservableObject
{
    public ObservableCollection<ILaunchScreenTabViewModel> Tabs { get; } = [];

    public ILogger? Logger { get; init; }

    public async Task OnDisplayedAsync(CancellationToken cancellationToken = default)
    {
        await foreach (
            var task in Task.WhenEach(Tabs.Select(x => x.OnDisplayedAsync(cancellationToken)))
                .WithCancellation(cancellationToken)
        )
        {
            if (task.IsFaulted)
            {
                Logger?.LogError(task.Exception, "Failed to display tab.");
            }
        }
    }
}
