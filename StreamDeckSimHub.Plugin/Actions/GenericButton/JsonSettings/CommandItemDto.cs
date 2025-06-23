// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

[JsonDerivedType(typeof(CommandItemKeypressDto), typeDiscriminator: "Keypress")]
[JsonDerivedType(typeof(CommandItemSimHubControlDto), typeDiscriminator: "SimHubControl")]
[JsonDerivedType(typeof(CommandItemSimHubRoleDto), typeDiscriminator: "SimHubRole")]
public abstract class CommandItemDto
{
    public required string Name { get; set; } = string.Empty;

    public required List<string> ActiveConditions { get; set; } = [];
}