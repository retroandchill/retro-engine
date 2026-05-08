// // @file NameCellEditFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.Services.Factories;

public class NameCellEditFactory : AbstractCellEditFactory
{
    public override Control? HandleNewProperty(PropertyCellContext context)
    {
        var propertyDescriptor = context.Property;

        if (propertyDescriptor.PropertyType != typeof(Name))
            return null;

        var control = new TextBox();
        control.TextChanged += (_, _) =>
        {
            SetAndRaise(context, control, (Name)control.Text!);
        };
        control.GotFocus += (_, _) =>
        {
            if (control.Text == "None")
            {
                control.Text = "";
            }
        };

        control.LostFocus += (_, _) =>
        {
            if (control.Text == "")
            {
                control.Text = "None";
            }
        };

        return control;
    }

    public override bool HandlePropertyChanged(PropertyCellContext context)
    {
        var propertyDescriptor = context.Property;
        var target = context.Target;
        var control = context.CellEdit!;

        if (propertyDescriptor.PropertyType != typeof(Name))
        {
            return false;
        }

        ValidateProperty(control, propertyDescriptor, target);
        if (control is not TextBox textBox)
            return false;

        var nameValue = (Name)propertyDescriptor.GetValue(target)!;
        if (nameValue.IsNone && textBox.IsFocused)
        {
            textBox.Text = "";
        }
        else
        {
            textBox.Text = nameValue;
        }
        return true;
    }
}
