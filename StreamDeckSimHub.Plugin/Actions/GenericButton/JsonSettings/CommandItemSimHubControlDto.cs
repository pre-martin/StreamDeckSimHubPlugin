// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class CommandItemSimHubControlDto : CommandItemDto
{
    public required string Control { get; set; } = string.Empty;

    public required bool LongEnabled { get; set; }
}