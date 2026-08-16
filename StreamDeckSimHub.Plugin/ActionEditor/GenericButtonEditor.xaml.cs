// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Xaml.Behaviors;
using NLog;
using StreamDeckSimHub.Plugin.ActionEditor.Behaviors;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

namespace StreamDeckSimHub.Plugin.ActionEditor;

public partial class GenericButtonEditor
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly string _actionUuid;

    [Obsolete("Only for XAML designer support. Use the constructor with actionUuid instead.",  false)]
    public GenericButtonEditor() : this(string.Empty)
    {
    }

    public GenericButtonEditor(string actionUuid)
    {
        _actionUuid = actionUuid;
        InitializeComponent();

        // Set up drag-and-drop delegates for the ListBoxes
        SetupDragDropBehaviors();
    }

    public void SetViewModel(SettingsViewModel settingsViewModel)
    {
        DataContext = settingsViewModel;
    }

    private void SetupDragDropBehaviors()
    {
        // Set up DisplayItems drag-and-drop behavior
        var displayItemsBehavior = Interaction.GetBehaviors(DisplayItemsListBox)
            .OfType<ListBoxDragDropBehavior>()
            .FirstOrDefault();
        if (displayItemsBehavior != null)
        {
            displayItemsBehavior.OnItemDropped = OnDisplayItemDropped;
        }

        // Set up CommandItems drag-and-drop behavior
        var commandItemsBehavior = Interaction.GetBehaviors(CommandItemsListBox)
            .OfType<ListBoxDragDropBehavior>()
            .FirstOrDefault();
        if (commandItemsBehavior != null)
        {
            commandItemsBehavior.CanDropFunc = CanDropCommandItem;
            commandItemsBehavior.OnItemDropped = OnCommandItemDropped;
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        GetWindow(this)?.AddHandler(
            PreviewMouseDownEvent,
            new MouseButtonEventHandler(CloseSettingsOnOutsideClick));
        GetWindow(this)!.LocationChanged += OnWindowLocationChanged;
        try
        {
            await ((SettingsViewModel)DataContext).FetchControlMapperRoles();
            await ((SettingsViewModel)DataContext).FetchShakeItBassProfiles();
            await ((SettingsViewModel)DataContext).FetchShakeItMotorsProfiles();
        }
        catch (Exception ex)
        {
            // No MessageBox here, because we don't want to disturb the user when opening the editor.
            _logger.Warn("Failed to fetch Control Mapper Roles and/or ShakeIt Profiles from SimHub. Is SimHub not running? Cause: " + ex.Message);
        }
    }

    private void OnWindowLocationChanged(object? sender, EventArgs e)
    {
        // When the window is being moved while the popup is open, the popup shall follow the window.
        if (SettingsPopup.IsOpen)
        {
            // Modify the offset and reset it again. This forces WPF to recalculate the popup position.
            var offset = SettingsPopup.HorizontalOffset;
            SettingsPopup.HorizontalOffset = offset + 1;
            SettingsPopup.HorizontalOffset = offset;
        }
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        ((SettingsViewModel)DataContext).IsSettingsOverlayVisible = false;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        GetWindow(this)?.RemoveHandler(
            PreviewMouseDownEvent,
            new MouseButtonEventHandler(CloseSettingsOnOutsideClick));
        GetWindow(this)!.LocationChanged -= OnWindowLocationChanged;
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

    /// <summary>
    /// Handles the drop operation for DisplayItems, updating the order in the view model and underlying model.
    /// </summary>
    private void OnDisplayItemDropped(object draggedItem, object targetItem, int sourceIndex, int targetIndex)
    {
        if (draggedItem is not DisplayItemViewModel) return;

        // Get the collection and reorder items
        ((SettingsViewModel)DataContext).DisplayItems.Move(sourceIndex, targetIndex);
        ((SettingsViewModel)DataContext).UpdateDisplayItemsOrder();
    }

    /// <summary>
    /// Validates if a CommandItem can be dropped on a target item.
    /// CommandItems can only be dropped on other CommandItems within the same StreamDeckAction group.
    /// </summary>
    private bool CanDropCommandItem(object draggedItem, object targetItem)
    {
        // If both are CommandItems, check if they're in the same StreamDeckAction group
        if (draggedItem is CommandItemViewModel draggedCommandItem && targetItem is CommandItemViewModel targetCommandItem)
        {
            return draggedCommandItem.ParentAction == targetCommandItem.ParentAction;
        }

        return false;
    }

    /// <summary>
    /// Handles the drop operation for CommandItems, updating the order in the view model and underlying model.
    /// </summary>
    private void OnCommandItemDropped(object draggedItem, object targetItem, int sourceIndex, int targetIndex)
    {
        if (draggedItem is not CommandItemViewModel commandItem) return;

        ((SettingsViewModel)DataContext).FlatCommandItems.Move(sourceIndex, targetIndex);
        ((SettingsViewModel)DataContext).UpdateCommandItemsOrder(commandItem.ParentAction);
    }

    /// <summary>
    /// Closes the Settings popup when the user clicks anywhere in the window outside the toggle button.
    /// </summary>
    private void CloseSettingsOnOutsideClick(object sender, MouseButtonEventArgs e)
    {
        var vm = (SettingsViewModel)DataContext;
        if (!vm.IsSettingsOverlayVisible) return;

        if (e.OriginalSource is DependencyObject source)
        {
            // Click on the toggle button itself: let the Click handler toggle the state.
            if (IsDescendantOf(source, SettingsButton)) return;
            // Click inside the popup content: keep the popup open.
            if (SettingsPopup.Child != null && IsDescendantOf(source, SettingsPopup.Child)) return;
        }

        vm.IsSettingsOverlayVisible = false;
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = (SettingsViewModel)DataContext;
        vm.IsSettingsOverlayVisible = !vm.IsSettingsOverlayVisible;
    }

    private static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        var current = element;
        while (current != null)
        {
            if (current == ancestor) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}