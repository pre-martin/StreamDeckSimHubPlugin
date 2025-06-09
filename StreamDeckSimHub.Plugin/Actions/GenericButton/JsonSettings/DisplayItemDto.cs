// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

[JsonDerivedType(typeof(DisplayItemImageDto), typeDiscriminator: "image")]
[JsonDerivedType(typeof(DisplayItemTextDto), typeDiscriminator: "text")]
[JsonDerivedType(typeof(DisplayItemValueDto), typeDiscriminator: "value")]
public abstract class DisplayItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayParameters")]
    public DisplayParametersDto DisplayParameters { get; set; } = new();

    [JsonPropertyName("visibilityConditions")]
    public List<string> VisibilityConditions { get; set; } = new();
}
