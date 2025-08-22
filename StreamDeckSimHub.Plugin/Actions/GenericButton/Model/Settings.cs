// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

/// <summary>
/// Model for the <c>GenericButton</c>.
/// </summary>
public partial class Settings : ObservableObject
{
    public static readonly Size NewActionKeySize = new(0, 0);

    [ObservableProperty] private string _name = string.Empty;

    /// <summary>
    /// Information about the key size on which these elements are used.
    /// </summary>
    public required Size KeySize { get; set; } = NewActionKeySize;

    /// <summary>
    /// List of elements that should be displayed.
    /// </summary>
    public ObservableCollection<DisplayItem> DisplayItems { get; } = [];

    /// <summary>
    /// Contains the list of actions for each possible Stream Deck action.
    /// </summary>
    public SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> CommandItems { get; } = new();

    public event PropertyChangedEventHandler? SettingsChanged;

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

            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayItems)));
        };

        foreach (var action in ModelDefinitions.GetCommandItemActions())
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

                SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandItems)));
            };
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        SettingsChanged?.Invoke(this, e);
    }
}