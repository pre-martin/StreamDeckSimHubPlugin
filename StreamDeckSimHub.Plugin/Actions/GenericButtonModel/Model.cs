// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButtonModel;

/// <summary>
/// Model for the <c>GenericButton</c>.
/// </summary>
public class Model
{
    /// <summary>
    /// Information about the device on which these elements are used.
    /// </summary>
    public StreamDeckKeyInfo KeyInfo { get; set; } = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    /// <summary>
    /// List of elements that should be displayed.
    /// </summary>
    public List<DisplayItem> DisplayItems { get; set; } = new();
}
