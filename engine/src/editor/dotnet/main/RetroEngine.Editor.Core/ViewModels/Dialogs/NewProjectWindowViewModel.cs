// // @file NewProjectWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Dialogs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Dialogs;

[ViewModelFor<NewProjectWindow>]
[RegisterTransient(Duplicate = DuplicateStrategy.Append)]
public partial class NewProjectWindowViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly IFileSystem _fileSystem;
    private readonly IDialogService _dialogService;
    private readonly ILogger<NewProjectWindowViewModel> _logger;

    [UsedImplicitly]
    public NewProjectWindowViewModel(
        IFileSystem fileSystem,
        IDialogService dialogService,
        ILogger<NewProjectWindowViewModel> logger
    )
    {
        _fileSystem = fileSystem;
        _dialogService = dialogService;
        _logger = logger;
        UpdateCanCreateValue();
    }

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
            || !_fileSystem.Directory.Exists(ProjectFolder)
        )
        {
            CanCreate = false;
            ErrorText = Text.AsLocalizable(TextNamespace, "InvalidProjectName", "Invalid project folder or name");
            return;
        }

        var projectPath = _fileSystem.Path.Combine(ProjectFolder, ProjectName);
        if (
            _fileSystem.Directory.Exists(projectPath)
            && _fileSystem.Directory.EnumerateFileSystemEntries(projectPath).Any()
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
                    _logger.LogError(t.Exception, "Failed to select project folder.");
                }
            });
    }

    private async Task SelectProjectFolderAsync()
    {
        var targetFolder = await _dialogService.ShowOpenFolderDialogAsync(this);
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

internal sealed class DesignNewProjectWindowViewModel() : NewProjectWindowViewModel(new FileSystem(), null!, null!);
