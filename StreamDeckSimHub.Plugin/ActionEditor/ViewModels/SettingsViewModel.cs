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
    public ObservableCollection<IFlatCommandItemsViewModel> FlatCommandItems { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommandItemCommand))]
    private IFlatCommandItemsViewModel? _selectedFlatCommandItem;

    public SettingsViewModel(Settings settings)
    {
        _settings = settings;

        DisplayItems = new ObservableCollection<DisplayItemViewModel>(
            settings.DisplayItems.Select(di => new DisplayItemViewModel(di))
        );

        foreach (var kvp in _settings.Commands)
        {
            FlatCommandItems.Add(new StreamDeckActionViewModel(kvp.Key));
            foreach (var commandItem in kvp.Value)
            {
                FlatCommandItems.Add(new CommandItemViewModel(commandItem, kvp.Key));
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
        var vm = new DisplayItemViewModel(displayItem);
        DisplayItems.Add(vm);
        SelectedDisplayItem = vm;
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

        _settings.Commands[action].Add(newItem);

        // Find the action in the flat list...
        var actionElement =
            FlatCommandItems.First(item => item is StreamDeckActionViewModel actionVm && actionVm.Action == action);
        // ... and skip all CommandItems - until we reach the next action element or the end. We have to insert right before.
        var index = FlatCommandItems.IndexOf(actionElement) + 1;
        while (index < FlatCommandItems.Count - 1 && FlatCommandItems[index] is CommandItemViewModel)
        {
            index++;
        }

        var vm = new CommandItemViewModel(newItem, action);
        FlatCommandItems.Insert(index, vm);
        SelectedFlatCommandItem = vm;
    }

    #endregion
}

public class DisplayItemViewModel(DisplayItem model) : ObservableObject
{
    private DisplayItem Model { get; } = model;
    public string Name => string.IsNullOrWhiteSpace(Model.Name) ? Model.GetType().Name : Model.Name;
}

/// Common interface for the flat command list with different entries.
public interface IFlatCommandItemsViewModel;

public class StreamDeckActionViewModel(StreamDeckAction action) : ObservableObject, IFlatCommandItemsViewModel
{
    public StreamDeckAction Action { get; } = action;

    public override string ToString() => Action.ToString();
}

public class CommandItemViewModel(CommandItem model, StreamDeckAction parentAction) : ObservableObject, IFlatCommandItemsViewModel
{
    private CommandItem Model { get; } = model;
    public StreamDeckAction ParentAction { get; } = parentAction;
    public string Name => Model.GetType().Name;
}