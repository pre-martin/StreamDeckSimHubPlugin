// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
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
    /// <param name="targetKeyInfo">The target key info.</param>
    /// <param name="displayItems">The display items to render.</param>
    /// <param name="blinkOverride">Optional blink override.</param>
    /// <param name="warning">When <c>true</c>, a warning icon is applied to the final image.</param>
    /// <returns>The image.</returns>
    Image<Rgba32> Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems, BlinkOverride? blinkOverride = null, bool warning = false);
}
