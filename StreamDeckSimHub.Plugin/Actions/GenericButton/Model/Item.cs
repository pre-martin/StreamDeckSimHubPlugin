// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class Item : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private ConditionsHolder _conditionsHolder = new();

    partial void OnConditionsHolderChanged(ConditionsHolder value)
    {
        value.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
        // No event handler on UsedProperties.CollectionChanged.
        // We rely only on the event of ConditionsHolder.ConditionString. This means that UsedProperties already has to contain
        // the new state when ConditionString is being updated.
        //value.UsedProperties.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ConditionsHolder.UsedProperties));
    }

    /// <summary>
    /// How shall the element be displayed/called in the UI?
    /// </summary>
    /// <remarks>The property always returns a string without line breaks.</remarks>
    public string DisplayName => LineBreakRegex().Replace(RawDisplayName, " ");

    /// <summary>
    /// How shall the element be displayed/called in the UI?
    /// </summary>
    /// <remarks>This method may return a string with line breaks. It will be removed by the public Getter.</remarks>
    protected abstract string RawDisplayName { get; }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex LineBreakRegex();
}