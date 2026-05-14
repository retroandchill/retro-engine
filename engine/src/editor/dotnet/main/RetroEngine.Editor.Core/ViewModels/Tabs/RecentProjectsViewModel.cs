// // @file RecentProjectsViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AutoViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using RetroEngine.Editor.Core.Model.ProjectStructure;
using RetroEngine.Editor.Core.Services.Actions;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<RecentProjectsTab>]
public sealed partial class RecentProjectsViewModel : ObservableObject, ILaunchScreenTabViewModel, IDisposable
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Tabs.RecentProjectsViewModel";
    private static readonly Text HeaderText = Text.AsLocalizable(TextNamespace, "Projects", "Projects");

    private readonly IRecentProjectsActions _recentProjectsActions;

    private readonly ObservableList<RecentProjectInfo> _recentProjects = [];

    public IReadOnlyList<RecentProjectInfo> RecentProjects { get; }

    public Text Header => HeaderText;

    /// <inheritdoc/>
    public RecentProjectsViewModel(IRecentProjectsActions recentProjectsActions)
    {
        _recentProjectsActions = recentProjectsActions;
        RecentProjects = _recentProjects.ToNotifyCollectionChanged();
        _recentProjectsActions.OnProjectDeleted += OnProjectDeleted;
    }

    public async Task OnDisplayedAsync(CancellationToken cancellationToken)
    {
        var projects = await _recentProjectsActions.GetRecentProjectsAsync(cancellationToken: cancellationToken);
        _recentProjects.Clear();
        _recentProjects.AddRange(projects);
    }

    [RelayCommand]
    private void NewProject()
    {
        _ = _recentProjectsActions.CreateNewProjectAsync();
    }

    [RelayCommand]
    private void OpenProject()
    {
        _ = _recentProjectsActions.OpenProjectAsync();
    }

    [RelayCommand]
    private void OpenRecentProject(RecentProjectInfo project)
    {
        _ = _recentProjectsActions.OpenRecentProjectAsync(project);
    }

    [RelayCommand]
    private void DeleteRecentProject(RecentProjectInfo project)
    {
        _ = _recentProjectsActions.DeleteRecentProjectAsync(project);
    }

    private void OnProjectDeleted(RecentProjectInfo project)
    {
        _recentProjects.Remove(project);
    }

    public void Dispose()
    {
        _recentProjectsActions.OnProjectDeleted -= OnProjectDeleted;
    }
}
