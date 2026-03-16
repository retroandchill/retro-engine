// // @file NewProjectWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Dialogs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Dialogs;

public delegate ValueTask<IDialogStorageFolder?> ShowOpenFolderDialogAsyncDelegate(NewProjectWindowViewModel viewModel);

[ViewModelFor<NewProjectWindow>]
public sealed partial class NewProjectWindowViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly IFileSystem _fileSystem;

    public NewProjectWindowViewModel()
    {
        _fileSystem = new FileSystem();

        UpdateCanCreateValue();
    }

    /// <inheritdoc/>
    public NewProjectWindowViewModel(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        UpdateCanCreateValue();
    }

    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Dialogs.NewProjectWindowViewModel";

    public bool? DialogResult
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public event EventHandler? RequestClose;

    public event Action<Exception>? OnProjectFolderError;

    private readonly ShowOpenFolderDialogAsyncDelegate? _showOpenFolderDialogAsync;
    public ShowOpenFolderDialogAsyncDelegate? ShowOpenFolderDialogAsync
    {
        init => _showOpenFolderDialogAsync = value;
    }

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

    public Text? ErrorText
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public bool CanCreate
    {
        get;
        private set => SetProperty(ref field, value);
    } = false;

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
                    OnProjectFolderError?.Invoke(t.Exception);
                }
            });
    }

    private async Task SelectProjectFolderAsync()
    {
        if (_showOpenFolderDialogAsync is null)
        {
            return;
        }

        var targetFolder = await _showOpenFolderDialogAsync(this);
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
