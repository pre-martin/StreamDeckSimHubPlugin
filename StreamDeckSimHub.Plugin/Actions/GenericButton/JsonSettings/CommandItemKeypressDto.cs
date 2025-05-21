// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class CommandItemKeypressDto : CommandItemDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("modifierCtrl")]
    public bool ModifierCtrl { get; set; }

    [JsonPropertyName("modifierAlt")]
    public bool ModifierAlt { get; set; }

    [JsonPropertyName("modifierShift")]
    public bool ModifierShift { get; set; }
}