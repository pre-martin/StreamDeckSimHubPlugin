// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public abstract class ItemDto
{
    public required string Name { get; set; } = string.Empty;

    public required string ConditionsString { get; set; } = string.Empty;

    public required Dictionary<string, List<ShakeItEntry>> ConditionsShakeItDictionary { get; set; } = new();
}
