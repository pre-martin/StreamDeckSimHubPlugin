// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemValueDto : DisplayItemDto
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    [JsonPropertyName("displayFormat")]
    public string DisplayFormat { get; set; } = string.Empty;

    [JsonPropertyName("font")]
    public string FontName { get; set; } = "Arial";

    [JsonPropertyName("fontSize")]
    public float FontSize { get; set; } = 12f;

    [JsonPropertyName("fontStyle")]
    public string FontStyle { get; set; } = nameof(SixLabors.Fonts.FontStyle.Regular);

    [JsonPropertyName("color")]
    public string Color { get; set; } = SixLabors.ImageSharp.Color.White.ToHex();
}
