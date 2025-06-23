// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly Settings _settings;

    /// List of DisplayItems from the model
    public ObservableCollection<DisplayItemViewModel> DisplayItems { get; }

    [ObservableProperty] private DisplayItemViewModel? _selectedDisplayItem;

    /// The Dictionary of StreamDeckKey to List of CommandItems from the model as a flat list.
    public ObservableCollection<IFlatCommandItemsViewModel> FlatCommandItems { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommandItemCommand))]
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
        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(IsAnyItemSelected));
    }

    partial void OnSelectedFlatCommandItemChanged(IFlatCommandItemsViewModel? value)
    {
        if (value != null) SelectedDisplayItem = null; // CommandItem selected -> no DisplayItem selected
        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(IsAnyItemSelected));
    }

    public SettingsViewModel(Settings settings)
    {
        _settings = settings;

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
        _settings.AddDisplayItem(displayItem);
        var vm = DisplayItemToViewModel(displayItem);
        DisplayItems.Add(vm);
        SelectedDisplayItem = vm;
    }

    private DisplayItemViewModel DisplayItemToViewModel(DisplayItem displayItem)
    {
        return displayItem switch
        {
            DisplayItemImage img => new DisplayItemImageViewModel(img),
            DisplayItemText txt => new DisplayItemTextViewModel(txt),
            DisplayItemValue val => new DisplayItemValueViewModel(val),
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

        _settings.AddCommandItem(action, newItem);

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
}

