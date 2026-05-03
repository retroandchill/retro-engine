// // @file EngineControlHostFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.Hosts;
using RetroEngine.Rendering;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton]
public sealed class EngineControlHostFactory(RenderManager renderManager)
{
    public EngineControlHost Create()
    {
        return new EngineControlHost(renderManager);
    }
}
