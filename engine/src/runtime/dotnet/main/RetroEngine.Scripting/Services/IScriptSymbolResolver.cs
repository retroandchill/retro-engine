// // @file IScriptSymbolResolver.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Scripting.Model;

namespace RetroEngine.Scripting.Services;

public interface IScriptSymbolResolver
{
    ScriptTypeDefinition? ResolveScriptType(ScriptTypeReference typeReference);

    ValueTask<ScriptTypeDefinition?> ResolveScriptTypeAsync(
        ScriptTypeReference typeReference,
        CancellationToken cancellationToken = default
    );
}
