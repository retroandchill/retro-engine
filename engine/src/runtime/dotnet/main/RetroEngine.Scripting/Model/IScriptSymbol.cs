// @file IScriptSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Scripting.Model;

public interface IScriptSymbol
{
    string Name { get; }

    public string FullName { get; }

    public string FullCodeName { get; }
}
