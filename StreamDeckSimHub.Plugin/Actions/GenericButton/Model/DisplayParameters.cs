// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class DisplayParameters
{
    public Point Position { get; set; } = new(0, 0);
    public float Transparency { get; set; } = 1f;
    public int Rotation { get; set; } = 0;
    public ScaleType Scale { get; set; } = ScaleType.None;
    public Size? Size { get; set; }
}
