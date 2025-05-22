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
    [ObservableProperty] private DisplayItemViewModel? _selectedDisplayItem;
    public ObservableCollection<CommandGroupViewModel> CommandGroups { get; }

    [ObservableProperty] private CommandGroupViewModel _selectedCommandGroup;

    public SettingsViewModel(Settings settings)
    {
        _settings = settings;

        DisplayItems = new ObservableCollection<DisplayItemViewModel>(
            settings.DisplayItems.Select(di => new DisplayItemViewModel(di)));

        CommandGroups = new ObservableCollection<CommandGroupViewModel>(
            settings.Commands.Select(kvp => new CommandGroupViewModel(kvp.Key, kvp.Value)));
        SelectedCommandGroup = CommandGroups[0];
    }

    #region AddDisplayItem

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

    private void AddImage()
    {
        var newItem = DisplayItemImage.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }

    private void AddText()
    {
        var newItem = DisplayItemText.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }

    private void AddValue()
    {
        var newItem = DisplayItemValue.Create();
        _settings.DisplayItems.Add(newItem);
        DisplayItems.Add(new DisplayItemViewModel(newItem));
    }

    #endregion

    #region AddCommandItem

    public ObservableCollection<string> CommandItemTypes { get; } =
    [
        CommandItemKeypress.UiName, CommandItemSimHubControl.UiName, CommandItemSimHubRole.UiName
    ];

    [ObservableProperty] private string _selectedAddCommandItemType = CommandItemKeypress.UiName;

    [RelayCommand]
    private void AddSelectedCommandItem()
    {
        switch (SelectedAddCommandItemType)
        {
            case CommandItemKeypress.UiName:
                AddKeypress();
                break;
            case CommandItemSimHubControl.UiName:
                AddSimHubControl();
                break;
            case CommandItemSimHubRole.UiName:
                AddSimHubRole();
                break;
        }
    }

    private void AddKeypress()
    {
        var newItem = CommandItemKeypress.Create();
        _settings.Commands[SelectedCommandGroup.Action].Add(newItem);
        CommandGroups.First(model => model.Action == SelectedCommandGroup.Action).Commands.Add(new CommandItemViewModel(newItem));
    }

    private void AddSimHubControl()
    {
        var newItem = CommandItemSimHubControl.Create();
        _settings.Commands[SelectedCommandGroup.Action].Add(newItem);
        CommandGroups.First(model => model.Action == SelectedCommandGroup.Action).Commands.Add(new CommandItemViewModel(newItem));
    }

    private void AddSimHubRole()
    {
        var newItem = CommandItemSimHubRole.Create();
        _settings.Commands[SelectedCommandGroup.Action].Add(newItem);
        CommandGroups.First(model => model.Action == SelectedCommandGroup.Action).Commands.Add(new CommandItemViewModel(newItem));
    }

    #endregion
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

    public CommandGroupViewModel(StreamDeckAction action, List<CommandItem> commands)
    {
        Action = action;
        Commands = new ObservableCollection<CommandItemViewModel>(commands.Select(item => new CommandItemViewModel(item)));
    }
}

public class CommandItemViewModel(CommandItem model) : ObservableObject
{
    private CommandItem Model { get; } = model;
    public string Name => Model.GetType().Name;
}