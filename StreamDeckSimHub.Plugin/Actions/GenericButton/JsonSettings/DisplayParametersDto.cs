// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayParametersDto
{
    [JsonPropertyName("position")]
    public PointDto Position { get; set; } = new();

    [JsonPropertyName("transparency")]
    public float Transparency { get; set; } = 1f;

    [JsonPropertyName("rotation")]
    public int Rotation { get; set; } = 0;

    [JsonPropertyName("scale")]
    public string Scale { get; set; } = nameof(ScaleType.None);

    [JsonPropertyName("size")]
    public SizeDto? Size { get; set; } = null;
}