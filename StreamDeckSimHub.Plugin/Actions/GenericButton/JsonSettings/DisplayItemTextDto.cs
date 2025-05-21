// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemTextDto : DisplayItemDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("font")]
    public string FontName { get; set; } = "Arial";

    [JsonPropertyName("fontSize")]
    public float FontSize { get; set; } = 12f;

    [JsonPropertyName("fontStyle")]
    public string FontStyle { get; set; } = nameof(SixLabors.Fonts.FontStyle.Regular);

    [JsonPropertyName("color")]
    public string Color { get; set; } = SixLabors.ImageSharp.Color.White.ToHex();
}