// // @file IScriptType.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Scripting.Model;

public interface IScriptType : IScriptSymbol
{
    public string FullCodeNameUnbound { get; }
}

public interface INamedScriptType : IScriptType
{
    public string Namespace { get; }
}
