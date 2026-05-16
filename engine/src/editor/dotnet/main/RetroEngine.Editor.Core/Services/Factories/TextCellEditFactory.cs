// @file TextCellEditFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using RetroEngine.Editor.Core.Views.Properties;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Services.Factories;

public sealed class TextCellEditFactory : AbstractCellEditFactory
{
    public override Control? HandleNewProperty(PropertyCellContext context)
    {
        var propertyDescriptor = context.Property;

        if (propertyDescriptor.PropertyType != typeof(Text))
            return null;

        var control = new TextInput();
        control.TextChanged += txt =>
        {
            SetAndRaise(context, control, control.Text);
        };
        return control;
    }

    public override bool HandlePropertyChanged(PropertyCellContext context)
    {
        var propertyDescriptor = context.Property;
        var target = context.Target;
        var control = context.CellEdit!;

        if (propertyDescriptor.PropertyType != typeof(Text))
        {
            return false;
        }

        ValidateProperty(control, propertyDescriptor, target);
        if (control is not TextInput textInput)
            return false;

        var textValue = (Text)propertyDescriptor.GetValue(target)!;
        textInput.Text = textValue;
        return true;
    }
}
