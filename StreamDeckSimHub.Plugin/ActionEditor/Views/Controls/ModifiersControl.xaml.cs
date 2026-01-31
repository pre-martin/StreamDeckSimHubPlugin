// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class ModifiersControl : UserControl
{
    public ModifiersControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the "Delete" button click for Modifiers.
    /// <p/>
    /// Implemented as code-behind and not as command, because this way the ModifierViewModel does not need to know its parent Settings.
    /// </summary>
    private void ModifierDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ModifierViewModel modifierViewModel })
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the modifier\n\"{modifierViewModel}\" ?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ((DisplayItemViewModel)DataContext).RemoveModifier(modifierViewModel);
            }
        }
    }
}