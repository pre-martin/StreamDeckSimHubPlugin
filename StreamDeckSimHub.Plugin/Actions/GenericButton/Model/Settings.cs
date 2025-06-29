// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

/// <summary>
/// Model for the <c>GenericButton</c>.
/// </summary>
public class Settings : ObservableObject
{
    /// <summary>
    /// Information about the key size on which these elements are used.
    /// </summary>
    public required Size KeySize { get; set; } = new(0, 0);

    /// TODO: Required?
    public StreamDeckKeyInfo KeyInfo { get; set; } = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    /// <summary>
    /// List of elements that should be displayed.
    /// </summary>
    public ObservableCollection<DisplayItem> DisplayItems { get; } = [];

    /// <summary>
    /// Contains the list of actions for each possible Stream Deck action.
    /// </summary>
    public SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> CommandItems { get; } = new();

    public event EventHandler? SettingsChanged;

    public Settings()
    {
        DisplayItems.CollectionChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);

        foreach (StreamDeckAction action in Enum.GetValues(typeof(StreamDeckAction)))
        {
            CommandItems[action] = [];
            CommandItems[action].CollectionChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddDisplayItem(DisplayItem displayItem)
    {
        DisplayItems.Add(displayItem);
        displayItem.PropertyChanged += (sender, args) => SettingsChanged?.Invoke(sender, args);
        displayItem.DisplayParameters.PropertyChanged += (sender, args) => SettingsChanged?.Invoke(sender, args);
    }

    public void AddCommandItem(StreamDeckAction action, CommandItem commandItem)
    {
        CommandItems[action].Add(commandItem);
        commandItem.PropertyChanged += (sender, args) => SettingsChanged?.Invoke(sender, args);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}