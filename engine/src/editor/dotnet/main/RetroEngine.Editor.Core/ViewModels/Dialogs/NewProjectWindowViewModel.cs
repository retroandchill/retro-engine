// // @file NewProjectWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Dialogs;
using RetroEngine.Portable.Localization;
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Dialogs;

[ViewModelFor<NewProjectWindow>]
public sealed partial class NewProjectWindowViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    public IFileSystem FileSystem
    {
        get;
        init
        {
            field = value;
            UpdateCanCreateValue();
        }
    } = IFileSystem.Default;
    public required IDialogService DialogService { get; init; }
    public ILogger? Logger { get; init; }

    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Dialogs.NewProjectWindowViewModel";

    [ObservableProperty]
    public partial bool? DialogResult { get; private set; }

    public event EventHandler? RequestClose;

    public string ProjectFolder
    {
        get;
        set
        {
            SetProperty(ref field, value);
            UpdateCanCreateValue();
        }
    } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public string ProjectName
    {
        get;
        set
        {
            SetProperty(ref field, value);
            UpdateCanCreateValue();
        }
    } = "NewProject";

    [ObservableProperty]
    public partial Text? ErrorText { get; private set; }

    [ObservableProperty]
    public partial bool CanCreate { get; private set; } = false;

    private void UpdateCanCreateValue()
    {
        if (
            string.IsNullOrWhiteSpace(ProjectFolder)
            || string.IsNullOrWhiteSpace(ProjectName)
            || !FileSystem.Directory.Exists(ProjectFolder)
        )
        {
            CanCreate = false;
            ErrorText = Text.AsLocalizable(TextNamespace, "InvalidProjectName", "Invalid project folder or name");
            return;
        }

        var projectPath = FileSystem.Path.Combine(ProjectFolder, ProjectName);
        if (
            FileSystem.Directory.Exists(projectPath)
            && FileSystem.Directory.EnumerateFileSystemEntries(projectPath).Any()
        )
        {
            CanCreate = false;
            ErrorText = Text.AsLocalizable(TextNamespace, "ProjectAlreadyExists", "Project directory is not empty");
            return;
        }

        CanCreate = true;
        ErrorText = null;
    }

    [RelayCommand]
    private void SelectProjectFolder()
    {
        SelectProjectFolderAsync()
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger?.LogError(t.Exception, "Failed to select project folder.");
                }
            });
    }

    private async Task SelectProjectFolderAsync()
    {
        var targetFolder = await DialogService.ShowOpenFolderDialogAsync(this);
        if (targetFolder is null)
            return;

        ProjectFolder = targetFolder.Path.ToString();
    }

    [RelayCommand]
    private void CreateProject()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
