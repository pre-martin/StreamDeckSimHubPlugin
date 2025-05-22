// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class CommandItemSimHubRole : CommandItem
{
    public const string UiName = "SimHub Role";

    public required string Role { get; set; } = string.Empty;

    public static CommandItemSimHubRole Create()
    {
        return new CommandItemSimHubRole
        {
            ActiveConditions = [],
            Role = string.Empty
        };
    }
}