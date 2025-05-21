// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class DisplayItemImageDto : DisplayItemDto
{
    [JsonPropertyName("relativePath")]
    public string RelativePath { get; set; } = string.Empty;
}
