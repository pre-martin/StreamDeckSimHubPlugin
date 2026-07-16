// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.ActionEditor.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemBoxDto : DisplayItemDto
{
    public required string Color { get; set; } = SixLabors.ImageSharp.Color.White.ToHexWithoutAlpha();

    public required int CornerRadius { get; set; }
}