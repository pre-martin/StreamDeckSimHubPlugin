// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

[JsonDerivedType(typeof(CommandItemKeypressDto), typeDiscriminator: "keypress")]
[JsonDerivedType(typeof(CommandItemSimHubControlDto), typeDiscriminator: "simhubcontrol")]
[JsonDerivedType(typeof(CommandItemSimHubRoleDto), typeDiscriminator: "simhubrole")]
public abstract class CommandItemDto
{
    [JsonPropertyName("activeConditions")]
    public List<string> ActiveConditions { get; set; } = new();
}