// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class SettingsDto
{
    [JsonPropertyName("keySize")]
    public SizeDto KeySize { get; set; } = new();

    [JsonPropertyName("displayItems")]
    public List<DisplayItemDto> DisplayItems { get; set; } = new();

    [JsonPropertyName("commands")]
    public Dictionary<string, SortedDictionary<int, CommandItemDto>> Commands { get; set; } = new();
}
