// // @file ProjectOpenViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services.Factories;

public interface IProjectOpenViewModelFactory
{
    ProjectOpenViewModel Create();
}
