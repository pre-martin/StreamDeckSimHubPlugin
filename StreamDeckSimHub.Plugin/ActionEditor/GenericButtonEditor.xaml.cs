// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor;

public partial class GenericButtonEditor
{
    private readonly string _actionUuid;

    public GenericButtonEditor(string actionUuid, Settings settings, ImageManager imageManager)
    {
        _actionUuid = actionUuid;
        InitializeComponent();
        DataContext = new SettingsViewModel(settings, imageManager, this);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new GenericButtonEditorClosedEvent(_actionUuid));
    }

    /// <summary>
    /// Handles the "Delete" button click for DisplayItems.
    /// <p/>
    /// Implemented as code-behind and not as command, because this way the DisplayItemViewModel does not need to know its parent Settings.
    /// </summary>
    private void DisplayItemDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: DisplayItemViewModel displayItemViewModel })
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the display item\n\"{displayItemViewModel.DisplayName}\" ?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ((SettingsViewModel)DataContext).RemoveDisplayItem(displayItemViewModel);
            }
        }
    }

    /// <summary>
    /// Handles the "Delete" button click for CommandItems.
    /// <p/>
    /// Implemented as code-behind and not as command, because this way the CommandItemViewModel does not need to know its parent Settings.
    /// </summary>
    private void CommandItemDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: CommandItemViewModel commandItemViewModel })
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the command item\n\"{commandItemViewModel.DisplayName}\" ?",
                "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ((SettingsViewModel)DataContext).RemoveCommandItem(commandItemViewModel);
            }
        }
    }
}