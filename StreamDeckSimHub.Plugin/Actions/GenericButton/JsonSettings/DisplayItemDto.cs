// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings.Modifiers;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

[JsonDerivedType(typeof(DisplayItemBoxDto), typeDiscriminator: "Box")]
[JsonDerivedType(typeof(DisplayItemImageDto), typeDiscriminator: "Image")]
[JsonDerivedType(typeof(DisplayItemTextDto), typeDiscriminator: "Text")]
[JsonDerivedType(typeof(DisplayItemValueDto), typeDiscriminator: "Value")]
public abstract class DisplayItemDto : ItemDto
{
    public required DisplayParametersDto DisplayParameters { get; set; } = new();

    public List<ModifierDto> Modifiers { get; set; } = [];
}