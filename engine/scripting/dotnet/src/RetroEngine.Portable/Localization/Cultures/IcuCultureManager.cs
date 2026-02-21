// // @file IcuCultureManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;

namespace RetroEngine.Portable.Localization.Cultures;

internal sealed partial class IcuCultureManager
{
    private readonly CultureManager _cultureManager;

    public IcuCultureManager(CultureManager cultureManager)
    {
        _cultureManager = cultureManager;

        // TODO: We may need to actually load our internationalization data from a reliable directory but for testing,
        // the system directories will do just fine
    }
}
