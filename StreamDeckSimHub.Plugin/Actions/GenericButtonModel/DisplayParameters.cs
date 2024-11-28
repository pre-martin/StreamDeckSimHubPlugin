// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButtonModel;

public class DisplayParameters
{

    public Point Position { get; set; }
    public float Transparency { get; set; } = 1f;
    public int Rotation { get; set; } = 0;
}