// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using StreamDeckSimHub.Plugin.ActionEditor.Behaviors;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class ModifiersControl : UserControl
{
    public ModifiersControl()
    {
        InitializeComponent();

        // Set up drag-drop delegates for the ListBoxes
        SetupDragDropBehaviors();
    }

    private void SetupDragDropBehaviors()
    {
        var modifierBehavior = Interaction.GetBehaviors(ModifiersListBox)
            .OfType<ListBoxDragDropBehavior>()
            .FirstOrDefault();
        if (modifierBehavior != null)
        {
            modifierBehavior.OnItemDropped = OnModifierDropped;
        }
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

    /// <summary>
    /// Handles the drop operation for Modifiers, updating the order in the view model and underlying model.
    /// </summary>
    private void OnModifierDropped(object draggedItem, object targetItem, int sourceIndex, int targetIndex)
    {
        if (draggedItem is not ModifierViewModel) return;

        // Get the collection and reorder items
        ((DisplayItemViewModel)DataContext).Modifiers.Move(sourceIndex, targetIndex);
        ((DisplayItemViewModel)DataContext).UpdateModifiersOrder();
    }
}