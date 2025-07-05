// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

/// <summary>
/// Delegate to retrieve a property value by its name.
/// </summary>
public delegate IComparable? GetPropertyDelegate(string propertyName);

public interface IButtonRenderer
{
    /// <summary>
    /// Renders all display items onto an image.
    /// </summary>
    /// <returns>The base64 encoded image.</returns>
    public string Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems);
}
