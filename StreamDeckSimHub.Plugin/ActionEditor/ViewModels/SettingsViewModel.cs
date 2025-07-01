// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly Settings _settings;
    private readonly ImageManager _imageManager;
    private readonly Window _parentWindow;

    /// List of DisplayItems (as ViewModels).
    public ObservableCollection<DisplayItemViewModel> DisplayItems { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedItem))]
    [NotifyPropertyChangedFor(nameof(IsAnyItemSelected))]
    private DisplayItemViewModel? _selectedDisplayItem;

    /// The Dictionary of StreamDeckKey to List of CommandItems (as ViewModels) as a flat list.
    public ObservableCollection<IFlatCommandItemsViewModel> FlatCommandItems { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommandItemCommand))]
    [NotifyPropertyChangedFor(nameof(SelectedItem))]
    [NotifyPropertyChangedFor(nameof(IsAnyItemSelected))]
    private IFlatCommandItemsViewModel? _selectedFlatCommandItem;

    /// <summary>
    /// Returns the currently selected DisplayItem or CommandItem, or null if none is selected.
    /// </summary>
    public object? SelectedItem => (object?)SelectedDisplayItem ?? SelectedFlatCommandItem as CommandItemViewModel;

    /// <summary>
    /// True if any DisplayItem or CommandItem is selected.
    /// </summary>
    public bool IsAnyItemSelected => SelectedItem != null;

    partial void OnSelectedDisplayItemChanged(DisplayItemViewModel? value)
    {
        if (value != null) SelectedFlatCommandItem = null; // DisplayItem selected -> no CommandItem selected
    }

    partial void OnSelectedFlatCommandItemChanged(IFlatCommandItemsViewModel? value)
    {
        if (value != null) SelectedDisplayItem = null; // CommandItem selected -> no DisplayItem selected
    }

    public SettingsViewModel(Settings settings, ImageManager imageManager, Window parentWindow)
    {
        _settings = settings;
        _imageManager = imageManager;
        _parentWindow = parentWindow;

        DisplayItems = new ObservableCollection<DisplayItemViewModel>(settings.DisplayItems.Select(DisplayItemToViewModel));

        foreach (var kvp in _settings.CommandItems)
        {
            FlatCommandItems.Add(new StreamDeckActionViewModel(kvp.Key));
            foreach (var commandItem in kvp.Value)
            {
                FlatCommandItems.Add(CommandItemToViewModel(commandItem, kvp.Key));
            }
        }
    }

    #region AddDisplayItem

    /// List of available display item types
    public ObservableCollection<string> DisplayItemTypes { get; } =
    [
        DisplayItemImage.UiName, DisplayItemText.UiName, DisplayItemValue.UiName
    ];

    [ObservableProperty] private string _selectedAddDisplayItemType = DisplayItemImage.UiName;

    [RelayCommand]
    private void AddSelectedDisplayItem()
    {
        switch (SelectedAddDisplayItemType)
        {
            case DisplayItemImage.UiName:
                AddDisplayItem(DisplayItemImage.Create());
                break;
            case DisplayItemText.UiName:
                AddDisplayItem(DisplayItemText.Create());
                break;
            case DisplayItemValue.UiName:
                AddDisplayItem(DisplayItemValue.Create());
                break;
        }
    }

    private void AddDisplayItem(DisplayItem displayItem)
    {
        _settings.DisplayItems.Add(displayItem);
        var vm = DisplayItemToViewModel(displayItem);
        DisplayItems.Add(vm);
        SelectedDisplayItem = vm;
    }

    private DisplayItemViewModel DisplayItemToViewModel(DisplayItem displayItem)
    {
        return displayItem switch
        {
            DisplayItemImage img => new DisplayItemImageViewModel(img, _imageManager, _parentWindow),
            DisplayItemText txt => new DisplayItemTextViewModel(txt, _parentWindow),
            DisplayItemValue val => new DisplayItemValueViewModel(val, _parentWindow),
            _ => throw new InvalidOperationException("Unknown DisplayItem type.")
        };
    }

    #endregion

    #region AddCommandItem

    /// List of available command item types
    public ObservableCollection<string> CommandItemTypes { get; } =
    [
        CommandItemKeypress.UiName, CommandItemSimHubControl.UiName, CommandItemSimHubRole.UiName
    ];

    [ObservableProperty] private string _selectedAddCommandItemType = CommandItemKeypress.UiName;

    [RelayCommand(CanExecute = nameof(CanExecuteAddCommandItem))]
    private void AddSelectedCommandItem()
    {
        switch (SelectedAddCommandItemType)
        {
            case CommandItemKeypress.UiName:
                AddCommandItem(CommandItemKeypress.Create());
                break;
            case CommandItemSimHubControl.UiName:
                AddCommandItem(CommandItemSimHubControl.Create());
                break;
            case CommandItemSimHubRole.UiName:
                AddCommandItem(CommandItemSimHubRole.Create());
                break;
        }
    }

    private bool CanExecuteAddCommandItem() => SelectedFlatCommandItem != null;

    private void AddCommandItem(CommandItem newItem)
    {
        // Determine the action of the currently selected list item
        var action = SelectedFlatCommandItem switch
        {
            StreamDeckActionViewModel actionVm => actionVm.Action,
            CommandItemViewModel itemVm => itemVm.ParentAction,
            _ => throw new InvalidOperationException("Invalid command list item selected.")
        };

        _settings.CommandItems[action].Add(newItem);

        // Find the action in the flat list...
        var actionElement =
            FlatCommandItems.First(item => item is StreamDeckActionViewModel actionVm && actionVm.Action == action);
        // ... and skip all CommandItems - until we reach the next action element or the end. We have to insert right before.
        var index = FlatCommandItems.IndexOf(actionElement) + 1;
        while (index < FlatCommandItems.Count - 1 && FlatCommandItems[index] is CommandItemViewModel)
        {
            index++;
        }

        var vm = CommandItemToViewModel(newItem, action);
        FlatCommandItems.Insert(index, vm);
        SelectedFlatCommandItem = vm;
    }

    private CommandItemViewModel CommandItemToViewModel(CommandItem commandItem, StreamDeckAction action)
    {
        return commandItem switch
        {
            CommandItemKeypress keypress => new CommandItemKeypressViewModel(keypress, action),
            CommandItemSimHubControl control => new CommandItemSimHubControlViewModel(control, action),
            CommandItemSimHubRole role => new CommandItemSimHubRoleViewModel(role, action),
            _ => throw new InvalidOperationException("Unknown CommandItem type.")
        };
    }

    #endregion

    #region RemoveItem

    public void RemoveDisplayItem(DisplayItemViewModel item)
    {
        // Remove from the underlying model
        var displayItem = (DisplayItem)item.GetModel();
        _settings.DisplayItems.Remove(displayItem);

        // Remove from the ViewModel collection
        DisplayItems.Remove(item);

        // Clear selection if this was the selected item
        if (SelectedDisplayItem == item)
        {
            SelectedDisplayItem = null;
        }
    }

    public void RemoveCommandItem(CommandItemViewModel commandItemViewModel)
    {
        // Remove from the underlying model
        var commandItem = (CommandItem)commandItemViewModel.GetModel();
        var action = commandItemViewModel.ParentAction;
        _settings.CommandItems[action].Remove(commandItem);

        // Remove from the ViewModel collection
        FlatCommandItems.Remove(commandItemViewModel);

        // Clear selection if this was the selected item
        if (SelectedFlatCommandItem == commandItemViewModel)
        {
            SelectedFlatCommandItem = null;
        }
    }

    #endregion

    #region DragDrop

    /// <summary>
    /// Updates the underlying model when DisplayItems are reordered
    /// </summary>
    public void UpdateDisplayItemsOrder()
    {
        // Update the underlying model's DisplayItems list to match the order in the ViewModel
        // We'll create a new list with the same items but in the new order
        var newList = new List<DisplayItem>();
        foreach (var displayItemVm in DisplayItems)
        {
            if (displayItemVm.GetModel() is DisplayItem model)
            {
                newList.Add(model);
            }
        }

        // Clear and repopulate the original list to maintain the reference
        _settings.DisplayItems.Clear();
        foreach (var item in newList)
        {
            _settings.DisplayItems.Add(item);
        }
    }

    /// <summary>
    /// Updates the underlying model when CommandItems are reordered within a StreamDeckAction
    /// </summary>
    public void UpdateCommandItemsOrder(StreamDeckAction action)
    {
        // Find all CommandItemViewModel instances for this action in the FlatCommandItems collection
        var commandItemVms = FlatCommandItems
            .OfType<CommandItemViewModel>()
            .Where(vm => vm.ParentAction == action)
            .ToList();

        // Create a new list with the items in the new order
        List<CommandItem> newList = commandItemVms
            .Select(vm => vm.GetModel() as CommandItem)
            .Where(item => item != null)
            .ToList()!;

        // Update the list in the dictionary (maintaining the reference)
        var existingList = _settings.CommandItems[action];
        existingList.Clear();
        foreach (var item in newList)
        {
            existingList.Add(item);
        }
    }

    #endregion
}
