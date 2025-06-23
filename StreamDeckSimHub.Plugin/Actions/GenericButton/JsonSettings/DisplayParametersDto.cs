// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayParametersDto
{
    public PointDto Position { get; set; } = new();

    public float Transparency { get; set; } = 1f;

    public int Rotation { get; set; } = 0;

    public string Scale { get; set; } = nameof(ScaleType.None);

    public SizeDto? Size { get; set; } = null;
}