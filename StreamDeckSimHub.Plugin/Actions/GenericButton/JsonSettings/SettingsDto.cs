// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class SettingsDto
{
    [JsonPropertyName("keySize")]
    public required SizeDto KeySize { get; set; } = new();

    [JsonPropertyName("displayItems")]
    public required List<DisplayItemDto> DisplayItems { get; set; } = new();

    [JsonPropertyName("commands")]
    public required Dictionary<string, List<CommandItemDto>> Commands { get; set; } = new();
}
