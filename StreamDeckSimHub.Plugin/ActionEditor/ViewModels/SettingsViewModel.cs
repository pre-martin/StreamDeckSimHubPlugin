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

    public ObservableCollection<DisplayItemViewModel> DisplayItems { get; }
    public ObservableCollection<CommandGroupViewModel> CommandGroups { get; }

    // List of available DisplayItem types
    public ObservableCollection<string> DisplayItemTypes { get; } =
    [
        DisplayItemImage.UiName,
        DisplayItemText.UiName,
        DisplayItemValue.UiName
    ];

    // Property to hold the selected DisplayItem
    [ObservableProperty] private string _selectedDisplayItemType = DisplayItemImage.UiName;

    public SettingsViewModel(Settings settings)
    {
        _settings = settings;

        DisplayItems = new ObservableCollection<DisplayItemViewModel>(
            settings.DisplayItems.Select(di => new DisplayItemViewModel(di)));

        CommandGroups = new ObservableCollection<CommandGroupViewModel>(
            settings.Commands.Select(kvp => new CommandGroupViewModel(kvp.Key, kvp.Value)));
    }

    // Command to add the selected DisplayItem
    [RelayCommand]
    private void AddSelectedDisplayItem()
    {
        switch (SelectedDisplayItemType)
        {
            case DisplayItemImage.UiName:
                AddImage();
                break;
            case DisplayItemText.UiName:
                AddText();
                break;
            case DisplayItemValue.UiName:
                AddValue();
                break;
        }
    }

    // Methods to create new DisplayItems with RelayCommand attribute
    [RelayCommand]
    private void AddImage()
    {
        var newItem = DisplayItemImage.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }

    [RelayCommand]
    private void AddText()
    {
        var newItem = DisplayItemText.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }

    [RelayCommand]
    private void AddValue()
    {
        var newItem = DisplayItemValue.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }
}

public class DisplayItemViewModel(DisplayItem model) : ObservableObject
{
    private DisplayItem Model { get; } = model;
    public string Name => string.IsNullOrWhiteSpace(Model.Name) ? Model.GetType().Name : Model.Name;
}

/// <summary>
/// Commands for a given StreamDeckAction.
/// </summary>
public class CommandGroupViewModel : ObservableObject
{
    public StreamDeckAction Action { get; }
    public ObservableCollection<CommandItemViewModel> Commands { get; }

    public CommandGroupViewModel(StreamDeckAction action, SortedDictionary<int, CommandItem> commands)
    {
        Action = action;
        Commands = new ObservableCollection<CommandItemViewModel>(commands
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new CommandItemViewModel(kvp.Value)));
    }
}

public class CommandItemViewModel(CommandItem model) : ObservableObject
{
    private CommandItem Model { get; } = model;
    public string Name => Model.GetType().Name;
}