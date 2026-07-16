// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings.Modifiers;

[JsonDerivedType(typeof(ModifierBlinkDto), typeDiscriminator: "Blink")]
[JsonDerivedType(typeof(ModifierColorDto), typeDiscriminator: "Color")]
public abstract class ModifierDto
{
    public required string ConditionsString { get; set; } = string.Empty;

    public required Dictionary<string, List<ShakeItEntry>> ConditionsShakeItDictionary { get; set; } = new();
}