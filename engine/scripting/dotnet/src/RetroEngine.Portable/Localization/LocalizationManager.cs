// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal class LocalizationManager
{
    private LocalizationManager() { }

    public static LocalizationManager Instance { get; } = new();

    public (ushort GlobalRevision, ushort LocalRevision) GetTextRevisions(TextId text)
    {
        throw new NotImplementedException();
    }
}
