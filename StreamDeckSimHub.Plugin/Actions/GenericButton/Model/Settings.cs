// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

/// <summary>
/// Model for the <c>GenericButton</c>.
/// </summary>
public class Settings
{
    /// <summary>
    /// Information about the key size on which these elements are used.
    /// </summary>
    public Size KeySize { get; set; } = new(0, 0);

    /// TODO: Required?
    public StreamDeckKeyInfo KeyInfo { get; set; } = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    /// <summary>
    /// List of elements that should be displayed.
    /// </summary>
    public required List<DisplayItem> DisplayItems { get; set; } = new();

    /// <summary>
    /// Contains the list of actions for each possible Stream Deck action.
    /// </summary>
    public required SortedDictionary<StreamDeckAction, List<CommandItem>> Commands { get; set; } = new();
}
