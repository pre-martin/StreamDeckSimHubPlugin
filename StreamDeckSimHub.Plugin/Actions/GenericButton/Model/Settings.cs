// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        DisplayItems.CollectionChanged += (_, args) =>
        {
            if (args is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                // Register on PropertyChanged of child DisplayItems, so that we can propagate these changes
                foreach (var item in args.NewItems)
                {
                    if (item is DisplayItem displayItem)
                    {
                        displayItem.PropertyChanged += (sender, a) => SettingsChanged?.Invoke(sender, a);
                        displayItem.DisplayParameters.PropertyChanged += (sender, a) => SettingsChanged?.Invoke(sender, a);
                    }
                }
            }

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        };

        foreach (StreamDeckAction action in Enum.GetValues(typeof(StreamDeckAction)))
        {
            CommandItems[action] = [];
            CommandItems[action].CollectionChanged += (_, args) =>
            {
                if (args is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
                {
                    // Register on PropertyChanged of child CommandItems, so that we can propagate these changes
                    foreach (var item in args.NewItems)
                    {
                        if (item is CommandItem commandItem)
                        {
                            commandItem.PropertyChanged += (sender, a) => SettingsChanged?.Invoke(sender, a);
                        }
                    }
                }

                SettingsChanged?.Invoke(this, EventArgs.Empty);
            };
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}