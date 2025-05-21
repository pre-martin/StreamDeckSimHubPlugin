// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract class CommandItem
{
    public List<ConditionExpression> ActiveConditions { get; set; } = [];
}