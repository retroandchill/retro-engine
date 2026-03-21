// // @file NewProjectWindowViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.ViewModels.Dialogs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class NewProjectWindowViewModelFactory(
    IFileSystem fileSystem,
    IDialogService dialogService,
    ILogger<NewProjectWindowViewModel> logger
) : ViewModelFactory<NewProjectWindowViewModel>
{
    public override NewProjectWindowViewModel CreateViewModel()
    {
        return new NewProjectWindowViewModel()
        {
            FileSystem = fileSystem,
            DialogService = dialogService,
            Logger = logger,
        };
    }
}
