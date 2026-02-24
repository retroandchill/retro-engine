// // @file LocalizedDockerContainerGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Dock.Model.Avalonia.Controls;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core;

public class LocalizedDockContainerGenerator : DockItemContainerGenerator
{
    private static readonly ImmutableArray<string> PossibleTitleProperties = ["Title", "Name", "DisplayName"];

    protected override string GetItemTitle(object item)
    {
        var type = item.GetType();

        foreach (var property in PossibleTitleProperties.Select(titleProperty => type.GetProperty(titleProperty)))
        {
            switch (property?.GetValue(item))
            {
                case string str:
                    return str;
                case Text text:
                    return text.ToString();
            }
        }

        return item.ToString() ?? type.Name;
    }
}
