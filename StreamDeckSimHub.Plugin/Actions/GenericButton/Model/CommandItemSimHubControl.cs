// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class CommandItemSimHubControl : CommandItem
{
    public const string UiName = "SimHub Control";

    public required string Control { get; set; } = string.Empty;

    public static CommandItemSimHubControl Create()
    {
        return new CommandItemSimHubControl
        {
            ActiveConditions = [],
            Control = string.Empty
        };
    }
}