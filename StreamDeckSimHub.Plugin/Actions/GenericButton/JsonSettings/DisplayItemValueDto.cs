// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemValueDto : DisplayItemDto
{
    [JsonPropertyName("property")]
    public required string Property { get; set; } = string.Empty;

    [JsonPropertyName("displayFormat")]
    public required string DisplayFormat { get; set; } = string.Empty;

    [JsonPropertyName("font")]
    public required string FontName { get; set; } = "Arial";

    [JsonPropertyName("fontSize")]
    public required float FontSize { get; set; } = 12f;

    [JsonPropertyName("fontStyle")]
    public required string FontStyle { get; set; } = nameof(SixLabors.Fonts.FontStyle.Regular);

    [JsonPropertyName("color")]
    public required string Color { get; set; } = SixLabors.ImageSharp.Color.White.ToHex();
}
