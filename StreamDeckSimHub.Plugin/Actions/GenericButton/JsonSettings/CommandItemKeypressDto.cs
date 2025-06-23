// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class CommandItemKeypressDto : CommandItemDto
{
    public required string Key { get; set; } = string.Empty;

    public required bool ModifierCtrl { get; set; }

    public required bool ModifierAlt { get; set; }

    public required bool ModifierShift { get; set; }
}