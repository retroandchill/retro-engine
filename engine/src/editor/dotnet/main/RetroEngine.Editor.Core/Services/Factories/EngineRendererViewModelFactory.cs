// // @file EngineControlHostFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Rendering;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class EngineRendererViewModelFactory(RenderManager renderManager)
    : ViewModelFactory<EngineRendererProvider>
{
    public override EngineRendererProvider CreateViewModel()
    {
        return new EngineRendererProvider(renderManager);
    }
}
