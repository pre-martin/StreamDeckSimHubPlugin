// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

public interface IButtonRenderer
{
    /// <summary>
    /// Context for logging information.
    /// </summary>
    void SetCoordinates(Coordinates coordinates);

    /// <summary>
    /// Renders all display items onto an image.
    /// </summary>
    /// <returns>The image.</returns>
    Image<Rgba32> Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems);
}
