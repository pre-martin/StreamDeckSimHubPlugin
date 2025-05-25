// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class CommandItemSimHubRoleDto : CommandItemDto
{
    [JsonPropertyName("role")]
    public required string Role { get; set; } = string.Empty;
}
