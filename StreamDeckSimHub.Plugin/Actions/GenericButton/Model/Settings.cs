// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
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
    public required ObservableCollection<DisplayItem> DisplayItems { get; set; } = new();

    /// <summary>
    /// Contains the list of actions for each possible Stream Deck action.
    /// </summary>
    public required SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> Commands { get; set; } = new();

    public event EventHandler? SettingsChanged;

    public Settings()
    {
        DisplayItems.CollectionChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        foreach (var item in DisplayItems)
        {
            if (item is ObservableObject oo)
                oo.PropertyChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        foreach (var kvp in Commands)
        {
            kvp.Value.CollectionChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
            foreach (var cmd in kvp.Value)
            {
                if (cmd is ObservableObject oo)
                    oo.PropertyChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
