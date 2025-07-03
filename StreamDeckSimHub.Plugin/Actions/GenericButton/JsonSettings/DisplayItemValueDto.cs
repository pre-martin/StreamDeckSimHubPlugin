// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemValueDto : DisplayItemDto
{
    public required string Property { get; set; } = string.Empty;

    public required string DisplayFormat { get; set; } = string.Empty;

    public required string FontName { get; set; } = "Arial";

    public required float FontSize { get; set; } = 20f;

    public required string FontStyle { get; set; } = nameof(SixLabors.Fonts.FontStyle.Regular);

    public required string Color { get; set; } = SixLabors.ImageSharp.Color.White.ToHexWithoutAlpha();
}